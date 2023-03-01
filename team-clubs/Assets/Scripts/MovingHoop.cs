using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingHoop : MonoBehaviour
{
    //Store spawn position
    //Get left cell position and right cell position
    //Move instantiated hoop from spawn point to other cell position
    //Lerp between the 2 positions

    private Vector3 posLeft;
    private Vector3 posRight;
    private bool goingLeft = true;
    public float posThreshold = 0.1f;
    public float movingSpeed = 2.0f;

    private void Start()
    {
        Vector3 gridIndex = GetComponentInChildren<Hoop>().CellIndex;
        Vector3 cellCounts = SpawnManager.Instance.GetCellCounts();
        if (gridIndex.x != 0)
        {
            gridIndex = new Vector3((gridIndex.x -1), gridIndex.y, gridIndex.z);
        }
        posLeft = SpawnManager.Instance.GetCellPosition((int)gridIndex.x, (int)gridIndex.y, (int)gridIndex.z);

        if (gridIndex.x != cellCounts.x-1)
        {
            gridIndex = new Vector3((gridIndex.x + 1), gridIndex.y, gridIndex.z);
        }
        posRight = SpawnManager.Instance.GetCellPosition((int)gridIndex.x, (int)gridIndex.y, (int)gridIndex.z);        

        //Store spawn point
        //spawnPos = transform.position;
        //Get other cell positions


    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != posLeft && goingLeft)
        {
            transform.position = Vector3.MoveTowards(transform.position, posLeft, Time.deltaTime * movingSpeed);
            
        }            

        if (transform.position != posRight && !goingLeft)
        {
            transform.position = Vector3.MoveTowards(transform.position, posRight, Time.deltaTime * movingSpeed);
        }

        if ((transform.position.x <= (posLeft.x + posThreshold) && goingLeft) || (transform.position.x >= (posRight.x - posThreshold) && !goingLeft))
            goingLeft = !goingLeft;
    }
}
