using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private List<Renderer> renderers;
    [SerializeField] private Color color = Color.white;

    private List<Material> materials;

    //renderer 가져오기
    private void Awake()
    {
        //자식오브젝트에 여러 개의 material이 있을 수 있기때문에 "s" 불어야함 materials
        materials = new List<Material>();
        foreach(var renderer in renderers)
        {
            materials.AddRange(new List<Material>(renderer.materials));
        }
    }

    public void ToggleHighlight(bool val)
    {
        if(val)
        {
            foreach(var material in materials)
            {
                //_EMISSION 활성화해야함
                material.EnableKeyword("_EMISSION");
                //색상 설정 전에
                material.SetColor("_EmissionColor", color);
            }
        }
        else
        {
            foreach(var material in materials)
            {
                //_EMISSION 비활성화
                //다른곳에서 _EMISSION색상을 사용하지 않는 경우
                material.DisableKeyword("_EMISSION");
            }
        }
    }
}
