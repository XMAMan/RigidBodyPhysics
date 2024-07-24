namespace PhysicEngine.MathHelper.PolygonDecomposition
{
    //Quelle: https://github.com/ivanfratric/polypartition/blob/master/src/polypartition.cpp#L968
    //23.10.2023: Ich habe von diesen Projekt die ConvexPartition_OPT-Methode genommen und sie nach C# übersetzt
    //Diese Funktion ist die Umsetzung für dieses Paper: On the Time Bound for Convex Decomposition of Simple Polygons - Keil and Snoeyink 1998.pdf
    internal static class PolygonDecomposer
    {
        private static void UpdateState(long a, long b, long w, long i, long j, DPState2[,] dpstates)
        {
            long w2 = dpstates[a, b].Weight;

            if (w > w2)
                return;

            var pairs = dpstates[a, b].Pairs;
            Diagonal newDiagonal = new Diagonal() { Index1 = i, Index2 = j };

            if (w < w2)
            {
                pairs.Clear();
                pairs.Insert(0, newDiagonal);
                dpstates[a, b].Weight = w;
            }
            else
            {
                if (pairs.Any() && i <= pairs.First().Index1)
                    return;

                while (pairs.Any() && pairs.First().Index2 >= j)
                    pairs.RemoveAt(0);

                pairs.Insert(0, newDiagonal);
            }
        }

        private static void TypeA(long i, long j, long k, PartitionVertex[] vertices, DPState2[,] dpstates)
        {
            if (!dpstates[i, j].Visible)
                return;

            long top = j;
            long w = dpstates[i, j].Weight;
            if (k - j > 1)
            {
                if (!dpstates[j, k].Visible)
                    return;
                w += dpstates[j, k].Weight + 1;
            }

            if (j - i > 1)
            {
                var pairs = dpstates[i, j].Pairs;
                int iter = pairs.Count;
                int lastiter = iter;
                while (iter > 0)
                {
                    iter--;
                    if (!PolyPointHelper.IsReflex(vertices[pairs[iter].Index2].P, vertices[j].P, vertices[k].P))
                        lastiter = iter;
                    else
                        break;
                }

                if (lastiter == pairs.Count)
                {
                    w++;
                }
                else
                {
                    long index1 = pairs[lastiter].Index1;
                    if (PolyPointHelper.IsReflex(vertices[k].P, vertices[i].P, vertices[index1].P))
                        w++;
                    else
                        top = index1;
                }
            }

            UpdateState(i, k, w, top, j, dpstates);
        }

        private static void TypeB(long i, long j, long k, PartitionVertex[] vertices, DPState2[,] dpstates)
        {
            if (!dpstates[j, k].Visible)
                return;

            long top = j;
            long w = dpstates[j, k].Weight;

            if (j - i > 1)
            {
                if (!dpstates[i, j].Visible)
                    return;

                w += dpstates[i, j].Weight + 1;
            }
            if (k - j > 1)
            {
                var pairs = dpstates[j, k].Pairs;

                int iter = 0;
                if (pairs.Any() && !PolyPointHelper.IsReflex(vertices[i].P, vertices[j].P, vertices[pairs[iter].Index1].P))
                {
                    int lastiter = iter;
                    while (iter != pairs.Count - 1)
                    {
                        if (!PolyPointHelper.IsReflex(vertices[i].P, vertices[j].P, vertices[pairs[iter].Index1].P))
                        {
                            lastiter = iter;
                            iter++;
                        }
                        else
                            break;
                    }

                    if (PolyPointHelper.IsReflex(vertices[pairs[lastiter].Index2].P, vertices[k].P, vertices[i].P))
                        w++;
                    else
                        top = pairs[lastiter].Index2;
                }
                else
                {
                    w++;
                }
            }

            UpdateState(i, k, w, j, top, dpstates);
        }

        public static IndexPoly[] DecomposePolygon(Poly poly)
        {
            if (poly.Valid() == false)
                throw new ArgumentException("poly is not valid");


            int n = poly.Points.Length;

            // Initialize vertex information.
            var vertices = new PartitionVertex[n];
            for (int i = 0; i < n; i++)
            {
                vertices[i] = new PartitionVertex()
                {
                    P = poly.Points[i],
                };
            }
            for (int i = 0; i < n; i++)
            {
                if (i == 0)
                    vertices[i].Previous = vertices.Last();
                else
                    vertices[i].Previous = vertices[i - 1];

                if (i == poly.Points.Length - 1)
                    vertices[i].Next = vertices[0];
                else
                    vertices[i].Next = vertices[i + 1];
            }

            for (int i = 1; i < n; i++)
                vertices[i].UpdateVertexReflexity();

            // Initialize states and visibility.
            var dpstates = new DPState2[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    dpstates[i, j] = new DPState2();

            for (int i = 0; i < (n - 1); i++)
            {
                var p1 = poly.Points[i];
                for (int j = i + 1; j < n; j++)
                {
                    dpstates[i, j].Visible = true;
                    dpstates[i, j].Pairs = new List<Diagonal>();
                    if (j == i + 1)
                        dpstates[i, j].Weight = 0;
                    else
                        dpstates[i, j].Weight = 2147483647;//Wenn hier long.MaxValue steht dann kommt ein falsches Ergebnis raus

                    if (j != (i + 1))
                    {
                        var p2 = poly.Points[j];

                        // Visibility check.
                        if (!vertices[i].InCone(p2))
                        {
                            dpstates[i, j].Visible = false;
                            continue;
                        }
                        if (!vertices[j].InCone(p1))
                        {
                            dpstates[i, j].Visible = false;
                            continue;
                        }

                        for (int k = 0; k < n; k++)
                        {
                            var p3 = poly.Points[k];
                            var p4 = k == n - 1 ? poly.Points[0] : poly.Points[k + 1];

                            if (PolyPointHelper.Intersects(p1, p2, p3, p4))
                            {
                                dpstates[i, j].Visible = false;
                                break;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < n - 2; i++)
            {
                int j = i + 2;
                if (dpstates[i, j].Visible)
                {
                    dpstates[i, j].Weight = 0;
                    dpstates[i, j].Pairs.Add(new Diagonal()
                    {
                        Index1 = i + 1,
                        Index2 = i + 1
                    });
                }
            }

            dpstates[0, n - 1].Visible = true;
            vertices[0].IsConvex = false; // By convention.

            for (int gap = 3; gap < n; gap++)
            {
                for (int i = 0; i < n - gap; i++)
                {
                    if (vertices[i].IsConvex)
                        continue;

                    int k = i + gap;
                    if (dpstates[i, k].Visible)
                    {
                        if (!vertices[i].IsConvex)
                        {
                            for (int j = i + 1; j < k; j++)
                                TypeA(i, j, k, vertices, dpstates);
                        }
                        else
                        {
                            for (int j = i + 1; j < (k - 1); j++)
                            {
                                if (vertices[j].IsConvex)
                                    continue;

                                TypeA(i, j, k, vertices, dpstates);
                            }
                            TypeA(i, k - 1, k, vertices, dpstates);
                        }
                    }
                }

                for (int k = gap; k < n; k++)
                {
                    if (vertices[k].IsConvex)
                        continue;

                    int i = k - gap;
                    if (vertices[i].IsConvex && dpstates[i, k].Visible)
                    {
                        TypeB(i, i + 1, k, vertices, dpstates);
                        for (int j = i + 2; j < k; j++)
                        {
                            if (vertices[j].IsConvex)
                                continue;

                            TypeB(i, j, k, vertices, dpstates);
                        }
                    }
                }
            }

            // Recover solution.
            int ret = 1;
            List<Diagonal> diagonals = new List<Diagonal>();
            diagonals.Add(new Diagonal() { Index1 = 0, Index2 = n - 1 });
            while (diagonals.Any())
            {
                var diagonal = diagonals.First();
                diagonals.RemoveAt(0);
                if (diagonal.Index2 - diagonal.Index1 <= 1)
                    continue;

                var pairs = dpstates[diagonal.Index1, diagonal.Index2].Pairs;
                if (pairs.Count == 0)
                {
                    ret = 0;
                    break;
                }
                if (!vertices[diagonal.Index1].IsConvex)
                {
                    int iter = pairs.Count - 1;
                    long j = pairs[iter].Index2;
                    diagonals.Insert(0, new Diagonal() { Index1 = j, Index2 = diagonal.Index2 });
                    if (j - diagonal.Index1 > 1)
                    {
                        if (pairs[iter].Index1 != pairs[iter].Index2)
                        {
                            var pairs2 = dpstates[diagonal.Index1, j].Pairs;
                            while (true)
                            {
                                if (pairs2.Count == 0)
                                {
                                    ret = 0;
                                    break;
                                }
                                int iter2 = pairs2.Count - 1;
                                if (pairs[iter].Index1 != pairs2[iter2].Index1)
                                    pairs2.RemoveAt(pairs2.Count - 1);
                                else
                                    break;
                            }
                            if (ret == 0)
                                break;
                        }
                        diagonals.Insert(0, new Diagonal() { Index1 = diagonal.Index1, Index2 = j });
                    }
                }
                else
                {
                    int iter = 0;
                    long j = pairs[iter].Index1;
                    diagonals.Insert(0, new Diagonal() { Index1 = diagonal.Index1, Index2 = j });
                    if (diagonal.Index2 - j > 1)
                    {
                        if (pairs[iter].Index1 != pairs[iter].Index2)
                        {
                            var pairs2 = dpstates[j, diagonal.Index2].Pairs;
                            while (true)
                            {
                                if (pairs2.Count == 0)
                                {
                                    ret = 0;
                                    break;
                                }
                                int iter2 = 0;
                                if (pairs[iter].Index2 != pairs2[iter2].Index2)
                                    pairs2.RemoveAt(0);
                                else
                                    break;
                            }
                            if (ret == 0)
                                break;
                        }
                        diagonals.Insert(0, new Diagonal() { Index1 = j, Index2 = diagonal.Index2 });
                    }
                }
            }

            if (ret == 0)
            {
                return new IndexPoly[] { new IndexPoly(poly.Points.Length) };
            }

            List<IndexPoly> parts = new List<IndexPoly>();
            diagonals.Insert(0, new Diagonal() { Index1 = 0, Index2 = n - 1 });
            while (diagonals.Any())
            {
                var diagonal = diagonals.First();
                diagonals.RemoveAt(0);
                if (diagonal.Index2 - diagonal.Index1 <= 1)
                    continue;

                List<long> indices = new List<long>();
                List<Diagonal> diagonals2 = new List<Diagonal>();

                indices.Add(diagonal.Index1);
                indices.Add(diagonal.Index2);
                diagonals2.Insert(0, diagonal);

                while (diagonals2.Any())
                {
                    diagonal = diagonals2.First();
                    diagonals2.RemoveAt(0);
                    if (diagonal.Index2 - diagonal.Index1 <= 1)
                        continue;

                    bool ijreal = true;
                    bool jkreal = true;
                    long j;
                    var pairs = dpstates[diagonal.Index1, diagonal.Index2].Pairs;
                    if (!vertices[diagonal.Index1].IsConvex)
                    {
                        int iter = pairs.Count - 1;
                        j = pairs[iter].Index2;
                        if (pairs[iter].Index1 != pairs[iter].Index2)
                            ijreal = false;
                    }
                    else
                    {
                        int iter = 0;
                        j = pairs[iter].Index1;
                        if (pairs[iter].Index1 != pairs[iter].Index2)
                            jkreal = false;
                    }

                    var newDiagonal = new Diagonal() { Index1 = diagonal.Index1, Index2 = j };
                    if (ijreal)
                        diagonals.Add(newDiagonal);
                    else
                        diagonals2.Add(newDiagonal);

                    newDiagonal = new Diagonal() { Index1 = j, Index2 = diagonal.Index2 };
                    if (jkreal)
                        diagonals.Add(newDiagonal);
                    else
                        diagonals2.Add(newDiagonal);

                    indices.Add(j);
                }

                indices.Sort();
                parts.Add(new IndexPoly(indices.ToArray()));
            }

            return parts.ToArray();
        }
    }
}
