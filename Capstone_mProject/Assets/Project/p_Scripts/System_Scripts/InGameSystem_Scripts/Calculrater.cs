using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    [Tooltip("데미지 표현식 (후위표기법 사용)")]
    public string damageExpression = "valueA valueB + valueC *";

    [Header("테스트 값")]
    public int valueA = 3;
    public int valueB = 4;
    public int valueC = 2;

    [ContextMenu("계산 및 출력")]
    void CalculateAndPrint()
    {
        try
        {
            int result = CalculateDamage(valueA, valueB, valueC);
            Debug.Log("데미지 계산 결과: " + result);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("데미지 계산 중 오류 발생: " + ex.Message);
        }
    }

    int CalculateDamage(int a, int b, int c)
    {
        string[] tokens = damageExpression.Split(' ');
        Stack<int> stack = new Stack<int>();

        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else if (token == "valueA")
            {
                stack.Push(a);
            }
            else if (token == "valueB")
            {
                stack.Push(b);
            }
            else if (token == "valueC")
            {
                stack.Push(c);
            }
            else
            {
                if (stack.Count < 2)
                {
                    throw new System.Exception("표현식이 올바르지 않습니다.");
                }

                int operand2 = stack.Pop();
                int operand1 = stack.Pop();

                switch (token)
                {
                    case "+":
                        stack.Push(operand1 + operand2);
                        break;
                    case "-":
                        stack.Push(operand1 - operand2);
                        break;
                    case "*":
                        stack.Push(operand1 * operand2);
                        break;
                    case "/":
                        if (operand2 == 0)
                        {
                            throw new System.Exception("0으로 나눌 수 없습니다.");
                        }
                        stack.Push(operand1 / operand2);
                        break;
                    default:
                        throw new System.Exception("지원하지 않는 연산자입니다.");
                }
            }
        }

        if (stack.Count != 1)
        {
            throw new System.Exception("표현식이 올바르지 않습니다.");
        }

        return stack.Pop();
    }
}
