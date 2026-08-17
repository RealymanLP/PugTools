using SlimDX;

namespace xxx
{
    public class Instance
    {
        // public xxx asset
        public ulong parent;

        public float posX;
        public float posY;
        public float posZ;

        public float rotX;
        public float rotY;
        public float rotZ;

        public float scaleX;
        public float scaleY;
        public float scaleZ;

        public uint width;
        public uint depth;

        public uint hook;

        public Matrix mvMatrix;
        public bool mvMatrixFixed;

        public Instance()
        {

        }
    }
}