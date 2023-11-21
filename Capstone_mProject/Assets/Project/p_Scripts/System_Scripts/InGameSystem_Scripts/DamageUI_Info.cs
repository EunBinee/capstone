using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageUI_Info : MonoBehaviour
{
    // 데미지 UI의 크기 조절.
    //데미지 크기에 따라서 폰트 사이즈 변경

    public Monster m_Monster;
    public TMP_Text txt_monsterDamage;
    public Vector3 m_DamagePos;

    public bool isReset = false;
    Coroutine size_co = null;
    public void Reset(Monster _monster, Vector3 pos, double damage)
    {


        if (damage > 1000) //임시 수치. 나중에 기획자가 변경할 수 있도록 수정.
        {
            Color l_Color = GameManager.Instance.HexToColor("#FF8C80");
            txt_monsterDamage.color = l_Color;
            txt_monsterDamage.fontSize = 800;
            int t_damage = (int)damage;
            txt_monsterDamage.text = t_damage.ToString();
        }
        else if (damage > 500)
        {
            Color m_Color = GameManager.Instance.HexToColor("#FFEC80");
            txt_monsterDamage.color = m_Color;
            txt_monsterDamage.fontSize = 600;
            int t_damage = (int)damage;
            txt_monsterDamage.text = t_damage.ToString();
        }
        else
        {
            Color s_Color = GameManager.Instance.HexToColor("#BAFF80");
            txt_monsterDamage.color = s_Color;
            txt_monsterDamage.fontSize = 400;
            int t_damage = (int)damage;
            txt_monsterDamage.text = t_damage.ToString();
        }

        m_Monster = _monster;
        m_DamagePos = pos;

        gameObject.SetActive(true);

        isReset = true;

        if (size_co != null)
            StopCoroutine(size_co);
        size_co = StartCoroutine(UI_ChangeSize());
    }

    IEnumerator UI_ChangeSize()
    {
        float lerpDuration = 0.3f;
        float elapsedTime = 0f;

        float startSize = txt_monsterDamage.fontSize;
        float endSize = startSize / 8;

        while (elapsedTime < lerpDuration)
        {
            // 시간 경과를 업데이트합니다.
            elapsedTime += Time.deltaTime;

            // 보간값을 계산합니다.
            float lerpedValue = Mathf.Lerp(startSize, endSize, elapsedTime / lerpDuration);

            txt_monsterDamage.fontSize = lerpedValue;

            yield return null;
        }

        yield return new WaitForSeconds(.15f);

        lerpDuration /= 3;
        elapsedTime = 0f;
        startSize = txt_monsterDamage.fontSize;
        while (elapsedTime < lerpDuration)
        {
            // 시간 경과를 업데이트합니다.
            elapsedTime += Time.deltaTime;

            // 보간값을 계산합니다.
            float lerpedValue = Mathf.Lerp(startSize, 0, elapsedTime / lerpDuration);

            txt_monsterDamage.fontSize = lerpedValue;

            yield return null;
        }

        //자기 자신 반납.
        GameManager.Instance.damageManager.Add_DamageUI(this);
        size_co = null;
    }

}
