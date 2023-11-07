using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    // ScriptableObject 로 생성한 스킬
    public SOSkill skill;

    // Player 객체 연결
    public PlayerController player;

    // 스킬 이미지
    public Image imgIcon;

    // Cooldown 이미지
    public Image imgCool;

    void Start()
    {
        // SO Skill 에 등록한 스킬 아이콘 연결
        imgIcon.sprite = skill.icon;

        // Cool 이미지 초기 설정
        imgCool.fillAmount = 0;
    }

    public void OnClicked()
    {
        // Cool 이미지의 fillAmount 가 0 보다 크다는 것은
        // 아직 쿨타임이 끝나지 않았다는 뜻
        if (imgCool.fillAmount > 0) return;

        // Player 객체의 ActivateSkill 호출     
        player.ActivateSkill(skill);

        // 스킬 Cool 처리
        StartCoroutine(SC_Cool());
    }

    IEnumerator SC_Cool()
    {
        // skill.cool 값에 따라 달라짐
        // 예: skill.cool 이 10초 라면
        // tick = 0.1
        float tick = 1f / skill.cool;
        float t = 0;

        imgCool.fillAmount = 1;

        // 10초에 걸쳐 1 -> 0 으로 변경하는 값을
        // imgCool.fillAmout 에 넣어주는 코드
        while (imgCool.fillAmount > 0)
        {
            imgCool.fillAmount = Mathf.Lerp(1, 0, t);
            t += (Time.deltaTime * tick);

            yield return null;
        }
    }
}