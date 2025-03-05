using UnityEngine;
using UnityEngine.UI;



namespace Invariable
{
    public class UIMask : MonoBehaviour
    {
        private Image[] m_image;
        private ParticleSystem[] m_particleSystem;
        private MeshRenderer[] m_meshRenderer;



        public void ShowMask()
        {
            m_image = GetComponentsInChildren<Image>();
            m_particleSystem = GetComponentsInChildren<ParticleSystem>();
            m_meshRenderer = GetComponentsInChildren<MeshRenderer>();

            CoroutineManager.Instance.LoadAddressables("Materials/UIMaskParent/UIMaskParentMaterial.mat", (asset) => {
                Material UIMaskParentMaterial = asset as Material;
                m_image[0].material = UIMaskParentMaterial;
            });

            CoroutineManager.Instance.LoadAddressables("Materials/UIMaskChildren/UIMaskChildrenMaterial.mat", (asset) => {
                Material UIMaskChildrenMaterial = asset as Material;

                if (m_image.Length > 1)
                {
                    for (int i = 1; i < m_image.Length; i++)
                    {
                        m_image[i].material = UIMaskChildrenMaterial;
                    }
                }

                if (m_particleSystem.Length > 0)
                {
                    for (int i = 0; i < m_particleSystem.Length; i++)
                    {
                        m_particleSystem[i].GetComponent<ParticleSystemRenderer>().material = UIMaskChildrenMaterial;
                    }
                }

                if (m_meshRenderer.Length > 0)
                {
                    for (int i = 0; i < m_meshRenderer.Length; i++)
                    {
                        m_meshRenderer[i].material = UIMaskChildrenMaterial;
                    }
                }
            });
        }
    }
}