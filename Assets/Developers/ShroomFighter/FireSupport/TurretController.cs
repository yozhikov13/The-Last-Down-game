using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    private bool IsActive, CanShoot;
    private Rigidbody rb;
    private float BulletRange=20f, RotationSpeed=7f;//Скорость поворота и дальность полета пули;
    static List<GameObject> EnemyInTrigger = new List<GameObject>();//список всех врагов, попавших в зону действия турели
    private GameObject  LastTarget;// последняя цель турели
    [Header("Поворот турели")]
    [SerializeField]
    private GameObject TurretSwivel;
    [Header("Наклон турели")]
    [SerializeField]
    private GameObject TurretBody;
    //float SwiwelAngle;//Угол между текущим и целевым поворотом башни
    //float BodyAngle;//Угол между текущим и целевым наклоном ствола
    Quaternion TargetQuaternionSwivel;
    Quaternion TargetQuaternionBody;
    [SerializeField]
    TurretShooting shootingcontroller;
    public IEnumerator Checkenemies;
    public Transform player;



    // Start is called before the first frame update
    void Start()
    {
        TargetQuaternionBody = TurretBody.transform.rotation;
        TargetQuaternionSwivel = TurretSwivel.transform.rotation;
        Checkenemies = CheckEnemies();
        LastTarget = null;
        rb = GetComponent<Rigidbody>();
        IsActive = CanShoot = false;
        StartCoroutine("CheckLand");
        
    }

    IEnumerator CheckLand()// Проверка на приземление 
    {
        while(true)
        {
            yield return new WaitForSeconds(0.2f);
            if (rb.velocity.x == 0 && rb.velocity.y == 0 && rb.velocity.z == 0)//Если турель не движется то
            {
                IsActive = true;//Активен
                break;
            }
            
        }
    }

    IEnumerator CheckEnemies()//проверка на возможность атакаковать и выбор цели
    {
        RaycastHit hit;
        float distance ;
        GameObject obj= null;
        while (true)
        {
            distance = 100;
            if (EnemyInTrigger.Count != 0)
            {
               foreach ( GameObject enemy in EnemyInTrigger)
                {
                   
                    Debug.DrawRay(TurretSwivel.transform.position, enemy.transform.position- TurretSwivel.transform.position, Color.green,1f);
                    if (Physics.Raycast(TurretSwivel.transform.position,enemy.transform.position - TurretSwivel.transform.position, out hit,BulletRange))
                    {
                       
                        
                        if (hit.collider.gameObject.GetComponent<HitBox>() != null)// Проверка на нахождение
                        {

                            if (hit.distance < distance )//противника за стеной и выбор противнка с наименьшей дистанцией, не учитывая текущую цель турели
                            {
                                
                                distance = hit.distance;
                                obj = enemy.gameObject;
                            }
                        }
                    }
                }
            }
            else break;
             if(obj != LastTarget)
            {
                LastTarget = obj;
               
                StopCoroutine("Rotate");
                StartCoroutine("Rotate", obj);
            }

            yield return new WaitForSeconds(0.5f);
        }



    }


    IEnumerator Rotate(GameObject target)// Наведение на цель
    {
        RaycastHit hit;
        Vector3 TargetVectorSwivel;//целевой вектор поворота башни
        Vector3 TargetVectorBody;//целевой вектор наклона пушки

        //поменять transform.position на TurretSwivel.transform.position
        while (true)
        {
            
            Debug.DrawRay(TurretSwivel.transform.position, target.transform.position- TurretSwivel.transform.position, Color.red,1f);
            if (Physics.Raycast(TurretSwivel.transform.position, target.transform.position- TurretSwivel.transform.position, out hit,BulletRange))
            {

                if (hit.collider.gameObject.GetComponent<HitBox>() != null)
                {

                    CanShoot = true;
                    TargetVectorSwivel = target.transform.position - TurretSwivel.transform.position;//Вычисление целевого вектора поворота башни(x,z)
                    TargetQuaternionSwivel = Quaternion.LookRotation(new Vector3(TargetVectorSwivel.x, 0, TargetVectorSwivel.z));

                    Debug.DrawRay(TurretSwivel.transform.position, new Vector3(TargetVectorSwivel.x, 0, TargetVectorSwivel.z) * 10, Color.yellow, 1f);

                    TargetVectorBody = target.transform.position - TurretBody.transform.position;//Вычисление целевого вектора наклона пушки(y,z)
                    TargetQuaternionBody = Quaternion.LookRotation(new Vector3(TargetVectorSwivel.x, TargetVectorBody.y, TargetVectorBody.z));

                    Debug.DrawRay(TurretBody.transform.position, new Vector3(TargetVectorSwivel.x, TargetVectorBody.y, TargetVectorBody.z).normalized * 10, Color.magenta, 1f);

                }
                else CanShoot = false;
            }
            
            yield return new WaitForSeconds(0.1f);

        }

    }

     

    private void Update()
    {
        if (IsActive)
        {
            
            if (TurretSwivel.transform.rotation!=TargetQuaternionSwivel&&TurretBody.transform.rotation!=TargetQuaternionBody)
            {
                shootingcontroller.EndShooting();
                TurretBody.transform.rotation = Quaternion.Lerp(TurretBody.transform.rotation, TargetQuaternionBody, RotationSpeed * Time.deltaTime); /*Quaternion.Lerp(TurretBody.transform.rotation, TargetQuaternionBody, RotationSpeed * Time.deltaTime);*/
                TurretSwivel.transform.rotation = Quaternion.Lerp(TurretSwivel.transform.rotation, TargetQuaternionSwivel, RotationSpeed * Time.deltaTime);
                // Debug.Log("swivel:" + TurretSwivel.transform.transform.rotation + "//TargetQuaternionSwivel:" + TargetQuaternionSwivel + "//body:" + TurretBody.transform.rotation + "//TargetQuaternionBody" + TargetQuaternionBody + "//Time" + Time.time);
            }
            else
            {
                if (CanShoot)
                {
                    Debug.Log("shoot" + Time.time);
                    shootingcontroller.StartShooting();
                }
            }

        }


    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterHitBox>() != null && player.name != other.transform.root.name && IsActive)
        {
           // Debug.Log(other.gameObject.name+"+++");
            EnemyInTrigger.Add(other.gameObject);
            if (EnemyInTrigger.Count == 1) StartCoroutine("CheckEnemies");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterHitBox>() != null&& player.name != other.transform.root.name)
        {
            if (other.gameObject == LastTarget)
            {
                CanShoot = false;
                StopCoroutine("Rotate");
                LastTarget = null;
            }
           // Debug.Log(other.gameObject.name + "---");
            EnemyInTrigger.Remove(other.gameObject);
        }
    }

   
}
