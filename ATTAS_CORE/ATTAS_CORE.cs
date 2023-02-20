﻿
using Google.OrTools.Sat;

namespace ATTAS_CORE
{
    public class SolutionPrinter : CpSolverSolutionCallback
    {
        private int solutionCount_;
        private int[] allTasks_;
        private int[] allInstructors_;
        private Dictionary<(int, int), BoolVar> assigns_;
        private int solutionLimit_;
        public SolutionPrinter(int[] allInstructorsWithBackup, int[] allTasks,
                               Dictionary<(int, int), BoolVar> assigns, int limit)
        {
            solutionCount_ = 0;
            allInstructors_ = allInstructorsWithBackup;
            allTasks_ = allTasks;
            assigns_ = assigns;
            solutionLimit_ = limit;
        }

        public override void OnSolutionCallback()
        {
            Console.WriteLine($"Solution #{solutionCount_}:");
            foreach (int n in allTasks_)
            {
                bool isAssigned = false;
                foreach (int i in allInstructors_)
                {
                    if (Value(assigns_[(n, i)]) == 1L)
                    {
                        isAssigned = true;
                        Console.WriteLine($" Task {n} assigned to instructor {i}");
                    }
                }
                if (!isAssigned)
                {
                    Console.WriteLine($" Task {n} need backup instructor!");
                }
                
            }
            solutionCount_++;
            if (solutionCount_ >= solutionLimit_)
            {
                Console.WriteLine($"Stop search after {solutionLimit_} solutions");
                StopSearch();
            }
        }

        public int SolutionCount()
        {
            return solutionCount_;
        }
    }
    public class ATTAS
    {
        /*
        ################################
        ||           MODEL            ||
        ################################
         */

        private CpModel model;
        // Desicion variable
        private Dictionary<(int, int), BoolVar> assigns;
        private Dictionary<(int, int), BoolVar> instructorSubjectStatus;
        private Dictionary<(int, int), LinearExpr> assignsProduct;
        public int testStart { get; set; } = 30;

        /*
        ################################
        ||           Option           ||
        ################################
        */
        public double maxSearchingTimeOption { get; set; } = 30.0;
        public string solverOption { get; set; } = "ORTOOLS";
        public string strategyOption { get; set; } = "CONSTRAINTPROGRAMMING";
        public int[] objOption { get; set; } = new int[6] { 1, 1, 1, 1, 1, 1 };

        /*
        ################################
        ||      SOLVER PARAMETER      ||
        ################################
        */

        //COUNT
        public int numSubjects { get; set; } = 0;
        public int numTasks { get; set; } = 0;
        public int numSlots { get; set; } = 0;
        public int numInstructors { get; set; } = 0;
        public int numBackupInstructors { get; set; } = 0;
        public int numAreas { get; set; } = 0;
        //RANGE
        private int[] allSubjects = Array.Empty<int>();
        private int[] allTasks = Array.Empty<int>();
        private int[] allSlots = Array.Empty<int>();
        private int[] allInstructors = Array.Empty<int>();
        private int[] allInstructorsWithBackup = Array.Empty<int>();
        private int[] allAreas = Array.Empty<int>();
        //INPUT DATA
        public int[,] slotConflict { get; set; } = new int[0, 0];
        public int[,] slotCompatibility { get; set; } = new int[0, 0];
        public int[,] instructorSubject { get; set; } = new int[0, 0];
        public int[,] instructorSubjectPreference { get; set; } = new int[0, 0];
        public int[,] instructorSlot { get; set; } = new int[0, 0];
        public int[,] instructorSlotPreference { get; set; } = new int[0, 0];
        public List<(int, int, int)> instructorPreassign { get; set; } = new List<(int, int, int)>();
        public int[] instructorQuota { get; set; } = Array.Empty<int>();
        public int[] taskSubjectMapping { get; set; } = Array.Empty<int>();
        public int[] taskSlotMapping { get; set; } = Array.Empty<int>();
        public int[] taskAreaMapping { get; set; } = Array.Empty<int>();
        public int[,] areaDistance { get; set; } = new int[0, 0];
        public int[,] areaSlotWeight { get; set; } = new int[0, 0];

        /*
        ################################
        ||          OR-TOOLS          ||
        ################################
        */
        public void setSolverCount()
        {
            allSubjects = Enumerable.Range(0, numSubjects).ToArray();
            allTasks = Enumerable.Range(0, numTasks).ToArray();
            allSlots = Enumerable.Range(0, numSlots).ToArray();
            allInstructors = Enumerable.Range(0, numInstructors).ToArray();
            allAreas = Enumerable.Range(0, numAreas).ToArray();

            if (numBackupInstructors > 0)
            {
                allInstructorsWithBackup = Enumerable.Range(0, numInstructors + 1).ToArray();
                instructorQuota = instructorQuota.Concat(new int[] { numBackupInstructors }).ToArray();
            }
            else
            {
                allInstructorsWithBackup = Enumerable.Range(0, numInstructors).ToArray();
            }
        }
        public void createModel()
        {
            model = new CpModel();

            assigns = new Dictionary<(int, int), BoolVar>();
            foreach (int n in allTasks)
                foreach (int i in allInstructorsWithBackup)
                {
                    assigns.Add((n, i), model.NewBoolVar($"n{n}i{i}"));
                }

            List<ILiteral> literals = new List<ILiteral>();
            //C-00 EACH TASK ASSIGN TO ATLEAST ONE AND ONLY ONE
            foreach (int n in allTasks)
            {
                foreach (int i in allInstructorsWithBackup)
                    literals.Add(assigns[(n, i)]);
                model.AddExactlyOne(literals);
                literals.Clear();
            }
            //C-00 CONSTRAINT INSTRUCTOR QUOTA MUST IN RANGE
            List<IntVar> taskAssigned = new List<IntVar>();
            foreach (int i in allInstructorsWithBackup)
            {
                foreach (int n in allTasks)
                    taskAssigned.Add(assigns[(n, i)]);
                model.AddLinearConstraint(LinearExpr.Sum(taskAssigned), 0, instructorQuota[i]);
                taskAssigned.Clear();
            }
            //C-01 NO SLOT CONFLICT
            List<LinearExpr> taskAssignedPerSlot = new List<LinearExpr>();
            foreach (int i in allInstructors)
                foreach (int s in allSlots)
                {
                    foreach (int n in allTasks)
                        taskAssignedPerSlot.Add(assigns[(n, i)] * slotConflict[taskSlotMapping[n], s]);
                    model.Add(LinearExpr.Sum(taskAssignedPerSlot) <= 1);
                    taskAssignedPerSlot.Clear();
                }
            //C-02 PREASSIGN MUST BE SATISFY
            foreach (var data in instructorPreassign)
            {
                if (data.Item3 == 1)
                    model.Add(assigns[(data.Item2, data.Item1)] == 1);
                if (data.Item3 == -1)
                    model.Add(assigns[(data.Item2, data.Item1)] == 0);
            }
            //C-03 INSTRUCTOR MUST HAVE ABILITY FOR THAT SUBJECT
            foreach (int n in allTasks)
                foreach (int i in allInstructors)
                    model.Add(instructorSubject[i, taskSubjectMapping[n]] - assigns[(n, i)] > -1);

            //C-04 INSTRUCTOR MUST BE ABLE TO TEACH IN THAT SLOT
            foreach (int n in allTasks)
                foreach (int i in allInstructors)
                    model.Add(instructorSlot[i, taskSlotMapping[n]] - assigns[(n, i)] > -1);
        }
        public void constraintOnly()
        {
            setSolverCount();
            createModel();
            CpSolver solver = new CpSolver();
            // Tell the solver to enumerate all solutions.
            solver.StringParameters += "linearization_level:0 " + "enumerate_all_solutions:true " + $"max_time_in_seconds:{maxSearchingTimeOption} ";

            // Display the first five solutions.
            const int solutionLimit = 1;
            SolutionPrinter cb = new SolutionPrinter(allInstructors, allTasks, assigns, solutionLimit);

            CpSolverStatus status = solver.Solve(model, cb);

            Console.WriteLine("Statistics");
            Console.WriteLine($"  status: {status}");
            Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
            Console.WriteLine($"  branches : {solver.NumBranches()}");
            Console.WriteLine($"  wall time: {solver.WallTime()}s");
        }
        /*
        ################################
        ||         OBJECTIVE          ||
        ################################
        */
        //O-01
        public LinearExpr objSlotCompatibilityCost()
        {
            List<LinearExpr> slotCompatibility_ = new List<LinearExpr>();
            for (int n1 = 0; n1 < numTasks - 1; n1++)
                for (int n2 = n1 + 1; n2 < numTasks; n2++)
                {
                    if (slotCompatibility[taskSlotMapping[n1], taskSlotMapping[n2]] == 0)
                        continue;
                    slotCompatibility_.Add(assignsProduct[(n1, n2)] * slotCompatibility[taskSlotMapping[n1], taskSlotMapping[n2]]);
                }
            return LinearExpr.Sum(slotCompatibility_);
        }
        //O-02
        public LinearExpr objSubjectDiversity()
        {
            List<ILiteral> literals = new List<ILiteral>();
            List<LinearExpr> subjectDiversity = new List<LinearExpr>();
            foreach (int i in allInstructors)
            {
                foreach (int s in allSubjects)
                    literals.Add(instructorSubjectStatus[(i, s)]);
                subjectDiversity.Add(LinearExpr.Sum(literals));
                literals.Clear();
            }
            IntVar obj = model.NewIntVar(0, numSubjects, "subjectDiversity");
            model.AddMaxEquality(obj, subjectDiversity);
            return obj;
        }
        //O-03
        public LinearExpr objQuotaReached()
        {
            List<LinearExpr> quotaDifference = new List<LinearExpr>();
            foreach (int i in allInstructors)
            {
                IntVar[] x = new IntVar[numTasks];
                foreach (int n in allTasks)
                    x[n] = assigns[(n, i)];
                quotaDifference.Add(instructorQuota[i] - LinearExpr.Sum(x));
            }
            IntVar obj = model.NewIntVar(0, numTasks, "maxQuotaDifference");
            model.AddMaxEquality(obj, quotaDifference);
            return obj;
        }
        //O-04
        public LinearExpr objWalkingDistance()
        {
            List<LinearExpr> walkingDistance = new List<LinearExpr>();
            for (int n1 = 0; n1 < numTasks - 1; n1++)
                for (int n2 = n1 + 1; n2 < numTasks; n2++)
                {
                    if (areaSlotWeight[taskSlotMapping[n1], taskSlotMapping[n2]] == 0 || areaDistance[taskAreaMapping[n1], taskAreaMapping[n2]] == 0)
                        continue;
                    walkingDistance.Add(assignsProduct[(n1, n2)] * areaSlotWeight[taskSlotMapping[n1], taskSlotMapping[n2]] * areaDistance[taskAreaMapping[n1], taskAreaMapping[n2]]);
                }
            return LinearExpr.Sum(walkingDistance);   
        }
        //O-05
        public LinearExpr objSubjectPreference()
        {
            IntVar[] assignedTasks = new IntVar[numTasks * numInstructors];
            int[] assignedTaskSubjectPreferences = new int[numTasks * numInstructors];
            foreach (int n in allTasks)
            {
                foreach (int i in allInstructors)
                {
                    assignedTasks[n * numInstructors + i] = assigns[(n, i)];
                    assignedTaskSubjectPreferences[n * numInstructors + i] = instructorSubjectPreference[i, taskSubjectMapping[n]];
                }
            }
            return LinearExpr.WeightedSum(assignedTasks, assignedTaskSubjectPreferences);
        }
        //O-06
        public LinearExpr objSlotPreference()
        {
            IntVar[] assignedTasks = new IntVar[numTasks * numInstructors];
            int[] assignedTaskSlotPreferences = new int[numTasks * numInstructors];
            foreach (int n in allTasks)
            {
                foreach (int i in allInstructors)
                {
                    assignedTasks[n * numInstructors + i] = assigns[(n, i)];
                    assignedTaskSlotPreferences[n * numInstructors + i] = instructorSlotPreference[i, taskSlotMapping[n]];
                }
            }
            return LinearExpr.WeightedSum(assignedTasks, assignedTaskSlotPreferences);
        }
        public void objectiveOptimize()
        {
            setSolverCount();
            createModel();
            CpSolver solver = new CpSolver();
            // Tell the solver to enumerate all solutions.
            solver.StringParameters += "linearization_level:0 " + $"max_time_in_seconds:{maxSearchingTimeOption} ";
            //O-03 MINIMIZE QUOTA DIFF
            if (objOption[2] > 0)
            {
                model.Minimize(objQuotaReached());
                CpSolverStatus status = solver.Solve(model);
                Console.WriteLine("Statistics");
                Console.WriteLine($"  Quota difference: {solver.ObjectiveValue}");
                Console.WriteLine($"  status: {status}");
                Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                Console.WriteLine($"  branches : {solver.NumBranches()}");
                Console.WriteLine($"  wall time: {solver.WallTime()}s");
            }
            //O-05 MAXIMIZE SUBJECT PREFERENCE
            if (objOption[4] > 0)
            {
                model.Maximize(objSubjectPreference());
                CpSolverStatus status = solver.Solve(model);
                Console.WriteLine("Statistics");
                Console.WriteLine($"  Subject Preference: {solver.ObjectiveValue}");
                Console.WriteLine($"  status: {status}");
                Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                Console.WriteLine($"  branches : {solver.NumBranches()}");
                Console.WriteLine($"  wall time: {solver.WallTime()}s");
            }
            //O-06 MAXIMIZE SLOT PREFERENCE
            if (objOption[5] > 0)
            {
                model.Maximize(objSlotPreference());
                CpSolverStatus status = solver.Solve(model);
                Console.WriteLine("Statistics");
                Console.WriteLine($"  Slot Preference: {solver.ObjectiveValue}");
                Console.WriteLine($"  status: {status}");
                Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                Console.WriteLine($"  branches : {solver.NumBranches()}");
                Console.WriteLine($"  wall time: {solver.WallTime()}s");
            }
            // O-02 MINIMIZE SUBJECT DIVERSITY
            if (objOption[1] > 0)
            {
                instructorSubjectStatus = new Dictionary<(int, int), BoolVar>();
                List<ILiteral> literals = new List<ILiteral>();
                foreach (int i in allInstructors)
                    foreach (int s in allSubjects)
                    {
                        foreach (int n in allTasks)
                            if (taskSubjectMapping[n] == s)
                                literals.Add(assigns[(n, i)]);
                        instructorSubjectStatus.Add((i, s), model.NewBoolVar($"i{i}s{s}"));
                        model.Add(LinearExpr.Sum(literals) > 0).OnlyEnforceIf(instructorSubjectStatus[(i, s)]);
                        model.Add(LinearExpr.Sum(literals) == 0).OnlyEnforceIf(instructorSubjectStatus[(i, s)].Not());
                        literals.Clear();
                    }

                model.Minimize(objSubjectDiversity());
                CpSolverStatus status = solver.Solve(model);
                Console.WriteLine("Statistics");
                Console.WriteLine($"  Subject Diversity: {solver.ObjectiveValue}");
                Console.WriteLine($"  status: {status}");
                Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                Console.WriteLine($"  branches : {solver.NumBranches()}");
                Console.WriteLine($"  wall time: {solver.WallTime()}s");
            }
            if (objOption[0] > 0 || objOption[3] > 0)
            {
                /*
                // OPTIMIZE IF POSSIBLE 
                // THIS IS x^2 OPTIMIZATION
                // MODEL SPEED DEPEND ON NUMBER OF VARIABLE
                // REDUCE VARIABLE IF POSSIBLE
                */
                assignsProduct = new Dictionary<(int, int), LinearExpr>();
                List<LinearExpr> mul = new List<LinearExpr>();
                List<LinearExpr> tmp = new List<LinearExpr>();
                for (int n1 = 0; n1 < numTasks-1;  n1++)
                    for (int n2 = n1+1; n2 < numTasks ; n2++)
                    {
                        /// REDUCE REDUNDANCE MODEL VARIABLE
                        if ( (areaSlotWeight[taskSlotMapping[n1], taskSlotMapping[n2]] == 0 || areaDistance[taskAreaMapping[n1], taskAreaMapping[n2]] == 0) && slotCompatibility[taskSlotMapping[n1], taskSlotMapping[n2]] == 0)
                            continue;
                        for (int i = testStart; i < numInstructors; i ++)
                        {
                            mul.Add(assigns[(n1, i)]);
                            mul.Add(assigns[(n2, i)]);
                            LinearExpr product = model.NewBoolVar("");
                            model.AddMultiplicationEquality(product, mul);
                            tmp.Add(product);
                            mul.Clear();
                        }
                        assignsProduct.Add((n1, n2), LinearExpr.Sum(tmp));
                        tmp.Clear();
                    }
            }
            //O-01 MAXIMIZE SLOT COMPATIBILITY
            if (objOption[0] > 0)
            {
                model.Minimize(objSlotCompatibilityCost());
                CpSolverStatus status = solver.Solve(model);
                Console.WriteLine("Statistics");
                Console.WriteLine($"  Slot Compatibility Cost: {solver.ObjectiveValue}");
                Console.WriteLine($"  status: {status}");
                Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                Console.WriteLine($"  branches : {solver.NumBranches()}");
                Console.WriteLine($"  wall time: {solver.WallTime()}s");
            }
            //O-04 MINIMIZE WALKING DISTANCE
            if (objOption[3] > 0 )
            {
                model.Minimize(objWalkingDistance());
                CpSolverStatus status = solver.Solve(model);
                Console.WriteLine("Statistics");
                Console.WriteLine($"  Walking Distance: {solver.ObjectiveValue}");
                Console.WriteLine($"  status: {status}");
                Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                Console.WriteLine($"  branches : {solver.NumBranches()}");
                Console.WriteLine($"  wall time: {solver.WallTime()}s");
            }
        }
        public void ortools()
        {
            if (objOption.Sum() == 0)
                constraintOnly();
            else
                objectiveOptimize();
        }
        /*
        ################################
        ||          MAIN HUB          ||
        ################################
        */
            public void solve()
        {
            switch (solverOption)
            {
                case "ORTOOLS":
                    ortools();
                    break;
            }
            
        }
    }
}