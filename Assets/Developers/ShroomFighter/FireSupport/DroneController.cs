using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    Queue<Vector3> FollowPoints;
    public float DroneSpeed;
    public Transform[] PointsAroundPlayer;
    private Vector3 TargetPoint;
    public Transform player;
    private Coroutine getfollowpoints,setrandompoint;
    bool Caught, CoroutineStarted,followpointsstarted;
    // Start is called before the first frame update
    void Start()
    {
        FollowPoints = new Queue<Vector3>();
        TargetPoint = transform.position;
        Caught =  true ;
        CoroutineStarted = false;
        StartCoroutine("CheckPlayer");

    }

    IEnumerator GetFollowPoints()
    {
        while (true)
        {
            
            yield return new WaitForSeconds(0.3f);
        }
    }

    

    IEnumerator CheckPlayer()
    {
        while(true)
        {
            
        Collider[] colliders= Physics.OverlapSphere(transform.position, 4);
        if (colliders.Length!=0)
        {
            bool HasPlayer = false;
            foreach(var i in colliders)
            {
                if (i.transform.root.name == player.name && i.transform.root.GetComponentInChildren<CharacterHitBox>() != null)
                {
                    HasPlayer = true;
                    break;
                }
            }
            if(HasPlayer)//игрок находится в поле видимости

            {
                    Debug.Log("Caught");
                    if (getfollowpoints != null) StopCoroutine(getfollowpoints);
                Caught = true;
                FollowPoints.Clear();
            }
            else//игрок ушел из  поля видимости
            {
                    Debug.Log("NotCaught");

                Caught = false;
                if (setrandompoint != null)
                {
                    StopCoroutine(setrandompoint);
                    CoroutineStarted = false;
                }
                if (FollowPoints.Count != 0)
                {
                    if (new Vector3(player.position.x, player.position.y + 2, player.position.z) != FollowPoints.ToArray()[FollowPoints.Count - 1])
                    {
                        //Debug.Log((player.position+"//"+ FollowPoints.ToArray()[FollowPoints.Count - 1]+"//"+(player.position- FollowPoints.ToArray()[FollowPoints.Count - 1]).magnitude));
                        //FollowPoints.ToArray()[FollowPoints.Count] player.position-FollowPoints.ToArray()[0]).magnitude<=1
                        FollowPoints.Enqueue(new Vector3(player.position.x, player.position.y + 2, player.position.z));
                        yield return new WaitForSeconds(0.3f);
                        continue;//проверка на  отстановку игрока. если стоит то записываем точки только после того как он опять начнет движение
                    }
                    //else Debug.Log(("@@@"+player.position + "//" + FollowPoints.ToArray()[FollowPoints.Count - 1] + "//" + (player.position - FollowPoints.ToArray()[FollowPoints.Count - 1]).magnitude));
                }
                else
                    FollowPoints.Enqueue(new Vector3(player.position.x, player.position.y + 1, player.position.z));
            }
        }
        else//игрок ушел из  поля видимости
        {
                Debug.Log("NotCaught");
                Caught = false;
            if (setrandompoint != null)
            {
                StopCoroutine(setrandompoint);
                CoroutineStarted = false;
            }
            if (FollowPoints.Count != 0)
            {
                if (new Vector3(player.position.x, player.position.y + 2, player.position.z) != FollowPoints.ToArray()[FollowPoints.Count - 1])
                {
                    //Debug.Log((player.position+"//"+ FollowPoints.ToArray()[FollowPoints.Count - 1]+"//"+(player.position- FollowPoints.ToArray()[FollowPoints.Count - 1]).magnitude));
                    //FollowPoints.ToArray()[FollowPoints.Count] player.position-FollowPoints.ToArray()[0]).magnitude<=1
                    FollowPoints.Enqueue(new Vector3(player.position.x, player.position.y + 2, player.position.z));
                    yield return new WaitForSeconds(0.3f);
                    continue;//проверка на  отстановку игрока. если стоит то записываем точки только после того как он опять начнет движение
                }
                //else Debug.Log(("@@@"+player.position + "//" + FollowPoints.ToArray()[FollowPoints.Count - 1] + "//" + (player.position - FollowPoints.ToArray()[FollowPoints.Count - 1]).magnitude));
            }
            else
                FollowPoints.Enqueue(new Vector3(player.position.x, player.position.y + 1, player.position.z));
        }
            yield return new WaitForSeconds(0.3f);
        }
    }

    Vector3 GetRandomPoint()
    {
        List<Vector3> AvaliablePoints = new List<Vector3>();
        foreach (var i in PointsAroundPlayer)
        {
            if (!Physics.Linecast(transform.position, i.position))
            {
                AvaliablePoints.Add(i.position);
            }
        }
        return AvaliablePoints[Random.Range(0, AvaliablePoints.Count )];
    }

    IEnumerator SetRandomPointToTarget()
    {
        
        while (true)
        {
            TargetPoint = GetRandomPoint();
            yield return new WaitForSeconds(Random.Range(1f, 5f));
        }
       


    }

    //private void OnTriggerEnter(Collider other)
    //{

    //    if (other.transform.root.GetComponentInChildren<CharacterHitBox>() != null && player.name == other.transform.root.name)
    //    {

    //        if(getfollowpoints!=null)StopCoroutine(getfollowpoints);
    //        Caught = true;
    //        FollowPoints.Clear();
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.transform.root.GetComponentInChildren<CharacterHitBox>() != null && player.name == other.transform.root.name )
    //    {

    //        getfollowpoints = StartCoroutine("GetFollowPoints");
    //        Caught = false;
    //        if (setrandompoint != null)
    //        {
    //            StopCoroutine(setrandompoint);
    //            CoroutineStarted = false;
    //        }
    //    }
    //}

    // Update is called once per frame
    void Update()
    {

        if (transform.position == TargetPoint)
        {
            if (!Caught)
            {

                if (FollowPoints.Count == 0)
                {
                    TargetPoint = transform.position;
                }
                else
                {
                    
                    TargetPoint = FollowPoints.Dequeue();
                }
            }
            else
            {
                if (!CoroutineStarted)
                {
                    CoroutineStarted = true;
                    setrandompoint = StartCoroutine("SetRandomPointToTarget");
                }
            }

        }
        transform.position = Vector3.MoveTowards(transform.position, TargetPoint, DroneSpeed * Time.deltaTime);
    }
}