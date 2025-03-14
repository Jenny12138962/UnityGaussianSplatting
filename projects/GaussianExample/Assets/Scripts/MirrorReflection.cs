using System;
using System.Collections.Generic;
using UnityEngine;

namespace CalmWater
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class MirrorReflection : MonoBehaviour
    {
        public LayerMask reflectionMask = -1;

        private enum QualityLevels
        {
            High = 1,
            Medium = 2,
            Low = 4,
            VeryLow = 8
        };

        [SerializeField]
        private QualityLevels Quality = QualityLevels.Medium;

        [Tooltip("Color used instead of skybox if you choose to not render it.")]
        public Color clearColor = Color.grey;
        public bool reflectSkybox = true;
        public bool m_DisablePixelLights = false;
        [Tooltip("You won't be able to select objects in the scene when this is active.")]
        public bool UpdateSceneView = true;
        public float clipPlaneOffset = 0.07f;

        private string reflectionSampler = "_ReflectionTex";

        private Camera m_ReflectionCamera;
        private Material m_SharedMaterial;
        private Dictionary<Camera, bool> m_HelperCameras;

        void OnEnable()
        {
            gameObject.layer = LayerMask.NameToLayer("Water");
            SetMaterial();
        }

        void OnDisable()
        {
            if (m_ReflectionCamera != null)
            {
                DestroyImmediate(m_ReflectionCamera.gameObject);
            }
        }

        void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Water");
            SetMaterial();
        }

        public void SetMaterial()
        {
            m_SharedMaterial = GetComponent<Renderer>().sharedMaterial;
        }

        Camera CreateReflectionCameraFor(Camera cam)
        {
            string reflName = $"{gameObject.name}Reflection{cam.name}";
            GameObject go = GameObject.Find(reflName);

            if (!go)
            {
                go = new GameObject(reflName, typeof(Camera));
                go.hideFlags = HideFlags.HideAndDontSave;
            }

            if (!go.TryGetComponent(out Camera reflectCamera))
            {
                reflectCamera = go.AddComponent<Camera>();
            }

            reflectCamera.CopyFrom(cam);
            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            reflectCamera.renderingPath = RenderingPath.Forward;
            reflectCamera.enabled = false;

            SetStandardCameraParameters(reflectCamera, reflectionMask);

            if (reflectCamera.targetTexture == null)
            {
                reflectCamera.targetTexture = CreateTextureFor(cam);
            }

            return reflectCamera;
        }

        void SetStandardCameraParameters(Camera cam, LayerMask mask)
        {
            cam.cullingMask = mask & ~(1 << LayerMask.NameToLayer("Water"));
            cam.depthTextureMode = DepthTextureMode.None;
        }

        RenderTexture CreateTextureFor(Camera cam)
        {
            int rtW = Mathf.FloorToInt(cam.pixelWidth / (int)Quality);
            int rtH = Mathf.FloorToInt(cam.pixelHeight / (int)Quality);
            RenderTexture rt = new RenderTexture(rtW, rtH, 24, RenderTextureFormat.DefaultHDR);
            rt.antiAliasing = 4;
            rt.hideFlags = HideFlags.DontSave;
            return rt;
        }

        public void RenderHelpCameras(Camera currentCam)
        {
            if (m_HelperCameras == null)
                m_HelperCameras = new Dictionary<Camera, bool>();

            if (!m_HelperCameras.ContainsKey(currentCam))
                m_HelperCameras.Add(currentCam, false);

            if (m_HelperCameras[currentCam] && !UpdateSceneView)
                return;

            if (m_ReflectionCamera == null)
                m_ReflectionCamera = CreateReflectionCameraFor(currentCam);

            RenderReflectionFor(currentCam, m_ReflectionCamera);
            m_HelperCameras[currentCam] = true;
        }

        public void LateUpdate()
        {
            if (m_HelperCameras != null)
                m_HelperCameras.Clear();
        }

        public void WaterTileBeingRendered(Transform tr, Camera currentCam)
        {
            RenderHelpCameras(currentCam);
            if (m_ReflectionCamera != null && m_SharedMaterial != null)
                m_SharedMaterial.SetTexture(reflectionSampler, m_ReflectionCamera.targetTexture);
        }

        public void OnWillRenderObject()
        {
            WaterTileBeingRendered(transform, Camera.current);
        }

        void RenderReflectionFor(Camera cam, Camera reflectCamera)
        {
            if (reflectCamera == null || (m_SharedMaterial != null && !m_SharedMaterial.HasProperty(reflectionSampler)))
                return;

#if UNITY_EDITOR
            // 动态调整反射纹理分辨率
            int rtW = Mathf.FloorToInt(cam.pixelWidth / (int)Quality);
            int rtH = Mathf.FloorToInt(cam.pixelHeight / (int)Quality);
            if (reflectCamera.targetTexture.width != rtW || reflectCamera.targetTexture.height != rtH)
            {
                DestroyImmediate(reflectCamera.targetTexture);
                reflectCamera.targetTexture = CreateTextureFor(cam);
            }
#endif

            // 保存原始像素灯光设置
            int originalPixelLightCount = QualitySettings.pixelLightCount;
            if (m_DisablePixelLights)
                QualitySettings.pixelLightCount = 0;

            // 设置反射相机参数
            reflectCamera.cullingMask = reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));
            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;

            // 精确镜像变换
            Vector3 planePosition = transform.position;
            Vector3 planeNormal = transform.up;
            Matrix4x4 reflectionMatrix = CalculatePreciseReflectionMatrix(planePosition, planeNormal);
            Vector3 reflectedPos = reflectionMatrix.MultiplyPoint(cam.transform.position);

            // 设置反射相机矩阵
            reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflectionMatrix;

            // 计算倾斜投影平面
            Vector4 clipPlane = CameraSpacePlane(reflectCamera, planePosition, planeNormal, 1.0f);
            reflectCamera.projectionMatrix = cam.projectionMatrix;
            reflectCamera.projectionMatrix = CalculatePreciseObliqueMatrix(reflectCamera.projectionMatrix, clipPlane);

            // 同步相机位置和旋转
            reflectCamera.transform.position = reflectedPos;
            reflectCamera.transform.rotation = Quaternion.LookRotation(
                Vector3.Reflect(cam.transform.forward, planeNormal),
                Vector3.Reflect(cam.transform.up, planeNormal)
            );

            // 渲染反射
            GL.invertCulling = true;
            reflectCamera.Render();
            GL.invertCulling = false;

            // 恢复像素灯光设置
            if (m_DisablePixelLights)
                QualitySettings.pixelLightCount = originalPixelLightCount;
        }

        // 高精度反射矩阵计算
        Matrix4x4 CalculatePreciseReflectionMatrix(Vector3 planePos, Vector3 planeNormal)
        {
            float d = -Vector3.Dot(planeNormal, planePos) - clipPlaneOffset;
            Matrix4x4 reflectionMat = Matrix4x4.identity;

            reflectionMat.m00 = 1 - 2 * planeNormal.x * planeNormal.x;
            reflectionMat.m01 = -2 * planeNormal.x * planeNormal.y;
            reflectionMat.m02 = -2 * planeNormal.x * planeNormal.z;
            reflectionMat.m03 = -2 * d * planeNormal.x;

            reflectionMat.m10 = -2 * planeNormal.y * planeNormal.x;
            reflectionMat.m11 = 1 - 2 * planeNormal.y * planeNormal.y;
            reflectionMat.m12 = -2 * planeNormal.y * planeNormal.z;
            reflectionMat.m13 = -2 * d * planeNormal.y;

            reflectionMat.m20 = -2 * planeNormal.z * planeNormal.x;
            reflectionMat.m21 = -2 * planeNormal.z * planeNormal.y;
            reflectionMat.m22 = 1 - 2 * planeNormal.z * planeNormal.z;
            reflectionMat.m23 = -2 * d * planeNormal.z;

            reflectionMat.m33 = 1;

            return reflectionMat;
        }

        // 高精度倾斜投影矩阵
        Matrix4x4 CalculatePreciseObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4(
                Mathf.Sign(clipPlane.x),
                Mathf.Sign(clipPlane.y),
                1.0f,
                1.0f
            );

            Vector4 c = clipPlane * (2.0f / Vector4.Dot(clipPlane, q));

            // 精确修改投影矩阵第三行
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];

            return projection;
        }

        // 摄像机空间平面计算（双精度中间计算）
        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(pos + normal * clipPlaneOffset);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

            // 使用双精度计算避免累计误差
            double dot = Vector3.Dot(cpos, cnormal);
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -(float)dot);
        }
    }
}