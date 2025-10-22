using Godot;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages
{
    public partial class GenerateTerrainStage : PipelineStage
    {
        // Stage parameters
        [ExportCategory("MapSize")]
        [Export] private int _rowLength = 100;
        [Export] private int _colLength = 100;
        
        [ExportCategory("Cellular Automata Params")]
        [Export] private float[] _stateProbabilities = new float[] { .4f, .6f };
        [Export] private int _walkableState = 0;
        [Export] private int _nonWalkableState = 1;
        [Export] private int _shrineState = -1;
        [Export] private int _smoothingNeighborDepth = 1;
        [Export] private bool _smoothingWraparound = false;
        [Export] private int _numSmooths = 5;

        private int[,] _matrix;
        private int[] _states;

        public override void ProcessWorld()
        {
            _matrix = new int[_rowLength, _colLength];
            _states = new int[] { _walkableState, _nonWalkableState };
            // trigger stage logic 
            Init();
            for (int i = 0; i < _numSmooths; i++)
                Smooth(_smoothingNeighborDepth, _smoothingWraparound);

            // update Global Data (Logical World)
            PipelineManager.LogicalWorldData.Matrix = _matrix;
            PipelineManager.LogicalWorldData.RowLength = _rowLength;
            PipelineManager.LogicalWorldData.ColLength = _colLength;
            PipelineManager.LogicalWorldData.States = _states;
            PipelineManager.LogicalWorldData.StateProbabilities = _stateProbabilities;
            PipelineManager.LogicalWorldData.WalkableState = _walkableState;
            PipelineManager.LogicalWorldData.NonWalkableState = _nonWalkableState;
            PipelineManager.LogicalWorldData.ShrineState = _shrineState;
        }

        private void Init()
        {
            for (int row = 0; row < _rowLength; row++)
            {
                for (int col = 0; col < _colLength; col++)
                {
                    float rInt = GD.Randf();
                    float cummulative = 0f;
                    for (int currState = 0; currState < _states.Length; currState++)
                    {
                        cummulative += _stateProbabilities[currState];
                        if (rInt <= cummulative)
                        {
                            _matrix[row, col] = _states[currState];
                            break;
                        }
                    }
                }
            }
        }

        private void Smooth(int neighborDepth, bool wrapAround)
        {
            int[,] copyArray = (int[,])_matrix.Clone();
            for (int row = 0; row < _rowLength; row++)
            {
                for (int col = 0; col < _colLength; col++)
                {
                    int newState =
                        MatrixUtils.MajorityNeighbor(_matrix, _states,
                            row, col, neighborDepth, wrapAround);
                    copyArray[row, col] = newState;
                }
            }

            _matrix = copyArray;
        }
    }
}