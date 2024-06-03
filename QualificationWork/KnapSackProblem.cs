using static QualificationWork.Program;

namespace QualificationWork {
    public class KnapSackProblem {
        public int Volume { get; set; }
        public Item[] Items { get; set; }
        public int MaxValue { get => _matrix == null ? 0 : _matrix[Items.Length, Volume]; }
        private List<Item> _solution;
        private int[,] _matrix;

        public KnapSackProblem(Item[] items, int volume) {
            Volume = volume;
            Items = items;
        }

        public List<Item> Solve() {
            _matrix = BuildTable();
            _solution = new List<Item>();
            FindItems2(Items.Length, Volume);
            return _solution;
        }
        private void FindItems2(int k, int s) {
            while (k > 0 && s > 0) {
                var current = _matrix[k, s];
                if (current != _matrix[k - 1, s]) {
                    _solution.Add(Items[k - 1]);
                    s -= Items[k - 1].Weight;
                }
                k--;
            }
        }

        //https://stackoverflow.com/questions/50393489/knapsack-c-sharp-implementation-task
        private int[,] BuildTable() {
            int[,] matrix = new int[Items.Length + 1, Volume + 1];
            for (int itemIndex = 0; itemIndex <= Items.Length; itemIndex++) {
                // This adjusts the itemIndex to be 1 based instead of 0 based
                // and in this case 0 is the initial state before an item is
                // considered for the knapsack.
                var currentItem = itemIndex == 0 ? null : Items[itemIndex - 1];
                for (int currentCapacity = 0; currentCapacity <= Volume; currentCapacity++) {
                    // Set the first row and column of the matrix to all zeros
                    // This is the state before any items are added and when the
                    // potential capacity is zero the value would also be zero.
                    if (currentItem == null || currentCapacity == 0) {
                        matrix[itemIndex, currentCapacity] = 0;
                    }
                    // If the current items weight is less than the current capacity
                    // then we should see if adding this item to the knapsack 
                    // results in a greater value than what was determined for
                    // the previous item at this potential capacity.
                    else if (currentItem.Weight <= currentCapacity) {
                        matrix[itemIndex, currentCapacity] = Math.Max(
                            currentItem.Value
                                + matrix[itemIndex - 1, currentCapacity - currentItem.Weight],
                            matrix[itemIndex - 1, currentCapacity]);
                    }
                    // current item will not fit so just set the value to the 
                    // what it was after handling the previous item.
                    else {
                        matrix[itemIndex, currentCapacity] =
                            matrix[itemIndex - 1, currentCapacity];
                    }
                }
            }
            // The solution should be the value determined after considering all
            // items at all the intermediate potential capacities.
            return matrix;
        }
    }
}