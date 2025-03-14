using UnityEngine;

namespace CalmWater
{
    public class MaterialSwitcher : MonoBehaviour
    {
        public MeshRenderer WaterPlane;
        public Material ClassicMat;
        public Material DX11Mat;

        private MirrorReflection mirrorRef;

        void Start()
        {
            mirrorRef = WaterPlane.GetComponent<MirrorReflection>();
        }

        public void SetDX11Mat()
        {
            WaterPlane.material = DX11Mat;
            mirrorRef.SetMaterial(); // 调用公开方法
        }

        public void SetClassicMat()
        {
            WaterPlane.material = ClassicMat;
            mirrorRef.SetMaterial(); // 调用公开方法
        }
    }
}