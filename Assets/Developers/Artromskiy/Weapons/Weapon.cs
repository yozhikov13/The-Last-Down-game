using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using DB;
using Mirror;

/// <summary>
/// Базовый класс для всех вариантов оружия
/// </summary>
public abstract class Weapon: MonoBehaviour
{
	public string Name;
    [SerializeField]
    [Range(0, 100)]
    /// <summary>
    /// Используется как параметр нанесения урона персонажу
    /// </summary>
    protected float damage;
    [SerializeField]
    [Range(0,90)]
    /// <summary>
    /// Параметр максимальной точности, влияет на смещение направления выстрела
    /// </summary>
    protected float accuracy = 90;
    [SerializeField]
    /// <summary>
    /// Текущая точность, расчитываемая на основе времени,
    /// событий выстрелов и параметра отдачи
    /// </summary>
    /// <value></value>
    protected float currentAccuracy;
    [SerializeField]
    [Range(0.016f, 10)]
    /// <summary>
    /// Время ожидания перед следующим выстрелом
    /// </summary>
    protected float rate = 0.016f;
    [SerializeField]
    [Range(0, 100)]
    /// <summary>
    /// Используется как параметр, ухудшающий текущую точность
    /// </summary>
    protected float recoil;
    [SerializeField]
    [Range(0, 90)]
    /// <summary>
    /// Используется как параметр, улучшающий текущую точность
    /// </summary>
    protected float control = 90;
    [SerializeField]
    [Range(0, 1000)]
    /// <summary>
    /// Максимальный магазин орудия
    /// </summary>
    private int magazine;
    [SerializeField]
    /// <summary>
    /// Текущее количество патронов в магазине.
    /// Используется для инициализации перезарядки.
    /// </summary>
    /// <value></value>
    protected int currentMagazine;
    [SerializeField]
    [Range(0, 1000)]
    /// <summary>
    /// Время проведения перезарядки
    /// </summary>
    protected float reloadTime;
    [SerializeField]
    [Range(0, 1000)]
    /// <summary>
    /// Используется как параметр для рэйкаста и т.д.
    /// </summary>
    protected float range;
    [SerializeField]
    [Range(0, 10)]
    /// <summary>
    /// Используется персонажем, как параметр влияющий на конечную скорость движения
    /// </summary>
    public float weight;
    [Space]
    [Header("Настройки для префаба")]
    [SerializeField]
    /// <summary>
    /// Точка произведения выстрела
    /// </summary>
    public Transform shootPoint;
    /// <summary>
    /// Используется, для описания логики стрельбы. Лучше представлять
    /// этот параметр, как нажатие спускового крючка орудия
    /// </summary>
    protected bool canShoot = false;
    /// <summary>
    /// Событие срабатывающее, когда зажат спусковой крючок, и время rate прошло
    /// </summary>
    public UnityEvent shootEvent;
    /// <summary>
    /// Событие срабатывающее при отсутствии патронов в магазине
    /// </summary>
    public UnityEvent reloadEvent;
    string Author;
    protected ShootInfo shootInfo;

    /// <summary>
    /// Стандартная инициализация. При перезаписывании
    /// сначала вызовите родительский метод, иначе
    /// закрытые слушающие методы будут утеряны
    /// </summary>
    public virtual void Start()
    {
				//deprecated
				//pClass = this.transform.root.gameObject.GetComponent<PlayerClass>();

        Author = GetComponentInParent<NetworkIdentity>()?.netId.ToString("G");
        if(!string.IsNullOrEmpty(Author))
        {
            shootInfo = new ShootInfo
            {
                AuthorInfo = Author,
                WeaponInfo = Name
            };
        }
        currentAccuracy = accuracy;
        currentMagazine = magazine;
        if(shootEvent==null)
        shootEvent = new UnityEvent();
        if(reloadEvent == null)
        reloadEvent = new UnityEvent();
        if(shootEvent != null)
        {
            shootEvent.AddListener(Shoot);
            shootEvent.AddListener(DoRecoil);
        }
				//StartCoroutine(SpecifyEvents());
	}

    private void OnEnable()
    {
        StartCoroutine(SpecifyEvents());
    }


    void Update()
    {
        SpecifyAccuracy();
    }

    /// <summary>
    /// Базовый метод для выстрела, определите в нём всю логику
    /// получения коллайдеров и вызовите метод Damage внутри.
    /// Этот метод будет автоматически вызываться через событие
    /// </summary>
    protected abstract void Shoot();
	/// <summary>
	/// Вызывайте при нажатии клавиши выстрела.
	/// Используйте, чтобы прописать логику изменения для переменной
	/// canShoot, или другой переменной со сходным применеием.
	/// </summary>
	public abstract void StartShooting();
    /// <summary>
    /// Вызывайте при отпускании клавиши выстрела.
    /// Используйте, чтобы прописать логику изменения для переменной
    /// canShoot, или другой переменной со сходным применением.
    /// </summary>
    public abstract void EndShooting();
    /// <summary>
    /// Основной метод, для нанесения урона, не следует переписывать,
    /// передавайте в него все коллайдеры, которым нужно нанести урон.
    /// </summary>
    /// <param name="colliders"></param>
    protected void Damage(Collider[] colliders)
    {
        for(int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;
            var hitBox = colliders[i].GetComponent<HitBox>();
            if(hitBox != null && this.transform.root.gameObject != hitBox.gameObject.transform.root.gameObject)
            {
                hitBox.TransportDamage(damage / colliders.Length, shootInfo, false);
            }
        }
    }

    protected Vector3 RandomRay()
    {
        var acc = ((90 - currentAccuracy) / 90);
        var rnd = Random.insideUnitCircle * acc * 45;
        var forwardrnd = Quaternion.Euler(rnd.x, rnd.y, 0) * shootPoint.forward;
        Debug.DrawRay(shootPoint.position, forwardrnd * range, Color.green, 1f);
		Debug.Log("Random Ray");
        return forwardrnd;
    }

    /// <summary>
    /// Уменьшает текущую точность на параметр отдачи.
    /// </summary>
    protected void DoRecoil()
    {
        // Выровнить отдачу, чтобы она была независима от скорострельности.
        var r = recoil * rate;
        currentAccuracy = currentAccuracy - r > 0 ? currentAccuracy - r : 0;
    }
    /// <summary>
    /// Устремляет параметр текущей точности к параметру
    /// максимальной точности на основании времени и силы отдачи.
    /// </summary>
    private void SpecifyAccuracy()
    {
        if(currentAccuracy == accuracy)
        {
            return;
        }
        if(currentAccuracy<accuracy)
        {
            currentAccuracy += control * Time.deltaTime;
        }
        if(currentAccuracy>accuracy)
        {
            currentAccuracy = accuracy;
        }
    }

    /// <summary>
    /// Вызывает события выстрела и перезарядки.
    /// </summary>
    /// <returns></returns>
    public IEnumerator SpecifyEvents()
    {
        while(true)
        {
            if(currentMagazine<=0)
            {
                reloadEvent.Invoke();
                yield return new WaitForSeconds(reloadTime);
                Reload();
            }
            if(canShoot)
            {
                currentMagazine--;
                shootEvent.Invoke(); // Вызов привязанных событий
                yield return new WaitForSeconds(rate);
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Вызывает срабатывание перезарядки в корутине
    /// т.к. корутина работает всегда и автоматически перезаряжает
    /// оружие при 0 патронов
    /// </summary>
    public void ForceReload()
    {
        currentMagazine = 0;
    }

    protected void Reload()
    {
        currentMagazine = magazine;
    }

    //обновление точности и кол-ва патронов в магазине при респавне
    public void RespawnCleaning()
    {
        currentMagazine = magazine;
        currentAccuracy = accuracy;
    }

    public class ShootInfo
    {
        public string AuthorInfo;
        public string WeaponInfo;
    }

#if (UNITY_EDITOR)
    public void SaveToDB()
    {
        WeaponParser wp = new WeaponParser()
        {
            Name = this.Name,
            Accuracy = accuracy,
            Damage = damage,
            Range = range,
            Rate = rate,
            Weight = weight,
            Recoil = recoil,
            Control = control,
            Magazine = magazine,
            ReloadTime = reloadTime,
            Type = this.GetType().ToString(),
            Slot = "Primary",
            MinLevel = 0,
            Price = 0,
            PremiumPrice = 0
        };
        DBAccessor.Context.SaveAsync(wp, (cal) =>
        {
            if (cal.Exception != null)
                Debug.Log(cal.Exception.Message);
            else
                Debug.Log($"Оружие {wp.Name} сохранено успешно");
        });
    }
#endif

#region deprecated
	public enum AvatarType
	{
		RIFLE,
		PISTOL
	}

	public Ray shootRay;
	private PlayerClass pClass;

	/// <summary>
	/// Тип оружия (двуручное/одноручное) для подключения соответсвующих анимаций на стороне сервера
	/// </summary>
	public AvatarType avatarType;

	public virtual void SetShootingRay(Ray ray)
	{

		shootRay = ray;

	}
#endregion
}
