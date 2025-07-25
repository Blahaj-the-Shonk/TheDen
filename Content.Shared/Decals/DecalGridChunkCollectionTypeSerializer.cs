// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Falcon <falcon@zigtag.dev>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Globalization;
using System.Linq;
using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using Robust.Shared.Utility;
using static Content.Shared.Decals.DecalGridComponent;

namespace Content.Shared.Decals
{
    [TypeSerializer]
    public sealed partial class DecalGridChunkCollectionTypeSerializer : ITypeSerializer<DecalGridChunkCollection, MappingDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return serializationManager.ValidateNode<Dictionary<Vector2i, Dictionary<uint, Decal>>>(node, context);
        }

        public DecalGridChunkCollection Read(ISerializationManager serializationManager,
            MappingDataNode node,
            IDependencyCollection dependencies, SerializationHookContext hookCtx, ISerializationContext? context = null,
            ISerializationManager.InstantiationDelegate<DecalGridChunkCollection>? _ = default)
        {
            node.TryGetValue("version", out var versionNode);
            var version = ((ValueDataNode?) versionNode)?.AsInt() ?? 1;
            Dictionary<Vector2i, DecalChunk> dictionary;

            // TODO: Dump this when we don't need support anymore.
            if (version > 1)
            {
                var nodes = (SequenceDataNode) node["nodes"];
                dictionary = new Dictionary<Vector2i, DecalChunk>();

                foreach (var dNode in nodes)
                {
                    var aNode = (MappingDataNode) dNode;
                    var data = serializationManager.Read<DecalData>(aNode["node"], hookCtx, context);
                    var deckNodes = (MappingDataNode) aNode["decals"];

                    foreach (var (decalUidNode, decalData) in deckNodes)
                    {
                        var dUid = uint.Parse(decalUidNode, CultureInfo.InvariantCulture);
                        var coords = serializationManager.Read<Vector2>(decalData, hookCtx, context);

                        var chunkOrigin = SharedMapSystem.GetChunkIndices(coords, SharedDecalSystem.ChunkSize);
                        var chunk = dictionary.GetOrNew(chunkOrigin);
                        var decal = new Decal(coords, data.Id, data.Color, data.Angle, data.ZIndex, data.Cleanable);
                        chunk.Decals.Add(dUid, decal);
                    }
                }
            }
            else
            {
                dictionary = serializationManager.Read<Dictionary<Vector2i, DecalChunk>>(node, hookCtx, context, notNullableOverride: true);
            }

            var uids = new SortedSet<uint>();
            var uidChunkMap = new Dictionary<uint, Vector2i>();
            var allIndices = dictionary.Keys.ToList();
            allIndices.Sort((x, y) => x.X == y.X ? x.Y.CompareTo(y.Y) : x.X.CompareTo(y.X));

            foreach (var indices in allIndices)
            {
                var decals = dictionary[indices];
                var decalUids = decals.Decals.Keys.ToList();
                decalUids.Sort();

                foreach (var uid in decalUids)
                {
                    uids.Add(uid);
                    uidChunkMap[uid] = indices;
                }
            }

            var uidMap = new Dictionary<uint, uint>();
            uint nextIndex = 0;
            foreach (var uid in uids)
            {
                uidMap[uid] = nextIndex++;
            }

            var newDict = new Dictionary<Vector2i, DecalChunk>();
            foreach (var (oldUid, newUid) in uidMap)
            {
                var indices = uidChunkMap[oldUid];
                if(!newDict.ContainsKey(indices))
                    newDict[indices] = new();
                newDict[indices].Decals[newUid] = dictionary[indices].Decals[oldUid];
            }

            return new DecalGridChunkCollection(newDict) { NextDecalId = nextIndex };
        }

        public DataNode Write(ISerializationManager serializationManager,
            DecalGridChunkCollection value, IDependencyCollection dependencies,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var lookup = new Dictionary<DecalData, List<uint>>();
            var decalLookup = new Dictionary<uint, Decal>();

            var allData = new MappingDataNode();
            // Want consistent chunk + decal ordering so diffs aren't mangled
            var chunks = new List<Vector2i>(value.ChunkCollection.Keys);
            chunks.Sort((x, y) => x.X == y.X ? x.Y.CompareTo(y.Y) : x.X.CompareTo(y.X));
            var nodes = new SequenceDataNode();

            // Assuming decal indices stay consistent:
            // We'll write decals by
            // - decaldata
            // - decal uid
            // - additional decal data

            // Build all of the decal lookups first.
            foreach (var chunk in value.ChunkCollection.Values)
            {
                var sortedDecals = new List<uint>(chunk.Decals.Keys);
                sortedDecals.Sort();

                foreach (var (uid, decal) in chunk.Decals)
                {
                    var data = new DecalData(decal);
                    var existing = lookup.GetOrNew(data);
                    existing.Add(uid);
                    decalLookup[uid] = decal;
                }
            }

            var lookupNodes = lookup.Keys.ToList();
            lookupNodes.Sort();

            foreach (var data in lookupNodes)
            {
                var uids = lookup[data];
                var lookupNode = new MappingDataNode { { "node", serializationManager.WriteValue(data, alwaysWrite, context) } };
                var decks = new MappingDataNode();

                uids.Sort();

                foreach (var uid in uids)
                {
                    var decal = decalLookup[uid];
                    // Inline coordinates
                    decks.Add(uid.ToString(), serializationManager.WriteValue(decal.Coordinates, alwaysWrite, context));
                }

                lookupNode.Add("decals", decks);
                nodes.Add(lookupNode);
            }

            allData.Add("version", 2.ToString(CultureInfo.InvariantCulture));
            allData.Add("nodes", nodes);

            return allData;
        }

        [DataDefinition]
        private readonly partial struct DecalData : IEquatable<DecalData>, IComparable<DecalData>
        {
            [DataField("id")]
            public string Id { get; init; } = string.Empty;

            [DataField("color")]
            public Color? Color { get; init; }

            [DataField("angle")]
            public Angle Angle { get; init; } = Angle.Zero;

            [DataField("zIndex")]
            public int ZIndex { get; init; }

            [DataField("cleanable")]
            public bool Cleanable { get; init; }

            public DecalData(string id, Color? color, Angle angle, int zIndex, bool cleanable)
            {
                Id = id;
                Color = color;
                Angle = angle;
                ZIndex = zIndex;
                Cleanable = cleanable;
            }

            public DecalData(Decal decal)
            {
                Id = decal.Id;
                Color = decal.Color;
                Angle = decal.Angle;
                ZIndex = decal.ZIndex;
                Cleanable = decal.Cleanable;
            }

            public bool Equals(DecalData other)
            {
                return Id == other.Id &&
                       Nullable.Equals(Color, other.Color) &&
                       Angle.Equals(other.Angle) &&
                       ZIndex == other.ZIndex &&
                       Cleanable == other.Cleanable;
            }

            public override bool Equals(object? obj)
            {
                return obj is DecalData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Color, Angle, ZIndex, Cleanable);
            }

            public int CompareTo(DecalData other)
            {
                var idComparison = string.Compare(Id, other.Id, StringComparison.Ordinal);
                if (idComparison != 0)
                    return idComparison;

                var colorComparison = string.Compare(Color?.ToHex(), other.Color?.ToHex(), StringComparison.Ordinal);

                if (colorComparison != 0)
                    return colorComparison;

                var angleComparison = Angle.Theta.CompareTo(other.Angle.Theta);

                if (angleComparison != 0)
                    return angleComparison;

                var zIndexComparison = ZIndex.CompareTo(other.ZIndex);
                if (zIndexComparison != 0)
                    return zIndexComparison;

                return Cleanable.CompareTo(other.Cleanable);
            }
        }
    }
}
