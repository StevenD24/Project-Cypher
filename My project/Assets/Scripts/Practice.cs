using UnityEngine;

public class Practice_x : MonoBehaviour
{
    // Variables
    // Types of variables: int, float, string, booleans, GameObjects
    // public/private type name val;
    public int moveSpeed = 0;
    public int A = 5;
    public int B = 3;
    private int C;

    // bread, soda, cost
    public float bread = 5.99F;
    public float soda = 1.49F;
    public float cost;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        C = A + B;
        Debug.Log("Number is: " + C);

        AddTwoNumbers(bread, soda);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddTwoNumbers(float a, float b)
    {
        cost = a + b;
        Debug.Log("The total cost is: " + cost);
    }
}
