// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Falcon <falcon@zigtag.dev>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using BenchmarkDotNet.Attributes;
using Robust.Shared.Analyzers;
using Robust.Shared.Maths;
using Robust.Shared.Physics;

namespace Content.Benchmarks
{
    [SimpleJob, MemoryDiagnoser]
    [Virtual]
    public class DynamicTreeBenchmark
    {
        private static readonly Box2[] Aabbs1 =
        {
            ((Box2) default).Enlarged(1), //2x2 square
            ((Box2) default).Enlarged(2), //4x4 square
            new(-3, 3, -3, 3), // point off to the bottom left
            new(-3, -3, -3, -3), // point off to the top left
            new(3, 3, 3, 3), // point off to the bottom right
            new(3, -3, 3, -3), // point off to the top right
            ((Box2) default).Enlarged(1), //2x2 square
            ((Box2) default).Enlarged(2), //4x4 square
            ((Box2) default).Enlarged(1), //2x2 square
            ((Box2) default).Enlarged(2), //4x4 square
            ((Box2) default).Enlarged(1), //2x2 square
            ((Box2) default).Enlarged(2), //4x4 square
            ((Box2) default).Enlarged(1), //2x2 square
            ((Box2) default).Enlarged(2), //4x4 square
            ((Box2) default).Enlarged(3), //6x6 square
            new(-3, 3, -3, 3), // point off to the bottom left
            new(-3, -3, -3, -3), // point off to the top left
            new(3, 3, 3, 3), // point off to the bottom right
            new(3, -3, 3, -3), // point off to the top right
        };

        private B2DynamicTree<int> _b2Tree;
        private DynamicTree<int> _tree;

        [GlobalSetup]
        public void Setup()
        {
            _b2Tree = new B2DynamicTree<int>();
            _tree = new DynamicTree<int>((in int value) => Aabbs1[value], capacity: 16);

            for (var i = 0; i < Aabbs1.Length; i++)
            {
                var aabb = Aabbs1[i];
                _b2Tree.CreateProxy(aabb, uint.MaxValue, i);
                _tree.Add(i);
            }
        }

        [Benchmark]
        public void BenchB2()
        {
            object state = null;
            _b2Tree.Query(ref state, (ref object _, DynamicTree.Proxy __) => true, new Box2(-1, -1, 1, 1));
        }

        [Benchmark]
        public void BenchQ()
        {
            foreach (var _ in _tree.QueryAabb(new Box2(-1, -1, 1, 1), true))
            {

            }
        }
    }
}
