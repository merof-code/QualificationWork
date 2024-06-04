using MathNet.Numerics.LinearAlgebra;

namespace QualificationWork {
    public partial class TimetableTask {
        //todo: do a converter to this List<ProfGroup> PlannedHours { get; set; }
        public int Days { get; private set; } = 5;
        public int HoursPerDay { get; private set; } = 6;
        public List<Professor> Professors { get; private set; }
        public List<Group> Groups { get; private set; }
        public Matrix<float> Matrix { get; private set; }

        private TimetableTask() {}

        public List<Item> ItemizeByProfessor(Matrix<float> weights, int j) {
            List<Item> list = new List<Item>((int)Matrix.RowSums().Sum());
            foreach (var (prof, hourCountRaw) in Matrix.EnumerateColumnsIndexed()) {
                foreach (var (group,hours) in hourCountRaw.EnumerateIndexed()) {
                    list.Add(new Item { Value = (int)weights[group, j], Weight = 1, Prof = j, Group = group });
                }
            }
            return list;
        }
        public List<Item> Itemize(Matrix<float> weights) {
            List<Item> list = new List<Item> ((int)Matrix.RowSums().Sum());
            foreach (var (group,prof,hourCountRaw) in Matrix.EnumerateIndexed()) {
                int hourCount = (int)hourCountRaw;
                for (int i = 0; i < hourCount; i++) {
                    list.Add(new Item { Value = (int)weights[group, prof], Weight=1, Prof = prof, Group = group });
                }
            }
            return list;
        }

    }
}