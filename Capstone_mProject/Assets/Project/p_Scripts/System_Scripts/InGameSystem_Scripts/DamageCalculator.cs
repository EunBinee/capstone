using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

[Serializable]
public class DamageCalculator
{

    [Tooltip("데미지 표현식 (중위표기법 사용)")]
    public string damageExpression = "A + B";

    [Header("테스트 값")]
    public int valueA = 350;
    public int valueB = 125;
    public int valueC = 500;

    //결과값
    public int result = 0;

    public void Init()
    {

    }

    //contextMenu 인스펙터창에서 실행 가능 
    [ContextMenu("계산 및 출력")]
    public void CalculateAndPrint()
    {
        // string postfixExpression = InfixToPostfix(damageExpression);
        // int result = EvaluatePostfix(postfixExpression);
        // Debug.Log("데미지 계산 결과: " + result);
        string postfixExpression = InfixToPostfix(damageExpression);
        //Debug.Log("후위표기법: " + postfixExpression); // 디버그용
        result = EvaluatePostfix(postfixExpression);
        //Debug.Log("데미지 계산 결과: " + result);
    }

    //중위표기법을 후위표기법으로 변환하는 함수. 
    private string InfixToPostfix(string infixExpression)
    {
        Dictionary<char, int> precedence = new Dictionary<char, int>
    {
        { '+', 1 }, { '-', 1 },
        { '*', 2 }, { '/', 2 },
        { '^', 3 }
    };

        Stack<char> stack = new Stack<char>();
        StringBuilder postfix = new StringBuilder();

        foreach (char ch in infixExpression)
        {
            if (char.IsLetterOrDigit(ch))
            {
                postfix.Append(ch);
            }
            else if (ch == ' ')
            {
                // 공백은 무시 
            }
            else if (ch == '(')
            {
                stack.Push(ch);
            }
            else if (ch == ')')
            {
                while (stack.Count > 0 && stack.Peek() != '(')
                {
                    postfix.Append(stack.Pop());
                }
                stack.Pop();  // '('는 버림 
            }
            else
            {
                //연산자 우선순위에 따라 후위표기법으로 변환 
                while (stack.Count > 0 && precedence.GetValueOrDefault(stack.Peek(), 0) >= precedence.GetValueOrDefault(ch, 0))
                {
                    postfix.Append(stack.Pop());
                }
                stack.Push(ch);
            }
        }

        while (stack.Count > 0)
        {
            postfix.Append(stack.Pop());
        }

        return postfix.ToString();
    }
    //후위표기법을 계산하는 함수 
    private int EvaluatePostfix(string postfixExpression)
    {
        Stack<int> stack = new Stack<int>();

        foreach (char ch in postfixExpression)
        {
            if (char.IsLetter(ch))
            {
                //변수에 해당하는 값 푸쉬
                switch (char.ToUpper(ch))
                {
                    case 'A':
                        stack.Push(valueA);
                        break;
                    case 'B':
                        stack.Push(valueB);
                        break;
                    case 'C':
                        stack.Push(valueC);
                        break;
                    default:
                        Debug.LogError("올바르지 않은 변수가 사용되었습니다.");
                        return 0;
                }
            }
            else if (char.IsDigit(ch))
            {
                stack.Push(int.Parse(ch.ToString()));
            }
            else
            {
                if (stack.Count < 2)
                {
                    Debug.LogError("피연산자가 부족합니다. 스택 상태: " + StackToString(stack));
                    return 0;
                }

                int operand2 = stack.Pop();
                int operand1 = stack.Pop();

                switch (ch)
                {
                    case '+':
                        stack.Push(operand1 + operand2);
                        break;
                    case '-':
                        stack.Push(operand1 - operand2);
                        break;
                    case '*':
                        stack.Push(operand1 * operand2);
                        break;
                    case '/':
                        if (operand2 == 0)
                        {
                            Debug.LogError("0으로 나눌 수 없습니다.");
                            return 0;
                        }
                        stack.Push(operand1 / operand2);
                        break;
                    default:
                        Debug.LogError("지원하지 않는 연산자입니다.");
                        return 0;
                }
            }
        }

        if (stack.Count == 1)
        {
            return stack.Pop();
        }
        else
        {
            Debug.LogError("피연산자가 부족합니다. 스택 상태: " + StackToString(stack));
            return 0;
        }
    }

    //스택을 문자열로 변환하는 함수 
    private string StackToString<T>(Stack<T> stack)
    {
        T[] array = stack.ToArray();
        System.Array.Reverse(array);
        return "[" + string.Join(", ", array) + "]";
    }

}
