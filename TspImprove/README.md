# TspImprove

Local-search module compiled into `TSP_CS.Core.dll` v2.0 (also usable on its own).

* `TspLocalSearch.BuildKnn(xy, k)` – k nearest neighbours for any point set (bucket grid, ~O(N·K)).
* `BucketGrid.Nearest(x, y, xy)` – nearest remaining city without scanning all cities.
* `TourOptimizer` – 2-opt + Or-opt with neighbour lists and a work queue, then Iterated Local Search
  (local double-bridge kicks; a kick is kept only if the tour gets shorter; undo log).
* `TspLocalSearch.Improve(tour, xy, nb, seconds)` – one call: local search, then ILS for `seconds`.

Test program: `TspImprove <file.xy> [ilsSeconds=2] [solverRoute.tsp]`
(.xy format: first line N, then one `x y` integer pair per line). It prints the nearest-neighbour tour,
the tour after 2-opt/Or-opt, and the tour after ILS. If a solver route file is given, it does the same from that route.
