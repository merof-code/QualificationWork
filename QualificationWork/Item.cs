namespace QualificationWork {
    public class Item {
        public int Weight { get; set; }
        public int Value { get; set; } = -1;
        public int Group { get; internal set; }
        public int Prof { get; internal set; }

        public override string ToString() {
            return $"(Group{Group+1} Prof{Group+1}|{Value})";
        }
    }
}


//create KnapSack problem for day one for prof 

//var s = new KnapSackProblem(items.ToArray(), backpackVolume);
//var solution = s.Solve();
//Console.WriteLine(string.Join(" ", solution));
//Console.WriteLine(s.MaxValue);