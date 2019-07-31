using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeSpriteController : MonoBehaviour {

    Dictionary<Employee, GameObject> EmployeeGameObjectMap;
    Dictionary<string, Sprite> EmployeeSpriteMap;
    

	// Use this for initialization
	void Start () {
        LoadSprites();
        EmployeeGameObjectMap = new Dictionary<Employee, GameObject>();
        World.Current.AddEmployeeCreatedCallback(OnEmployeeCreated);
        World.Current.AddEmployeeRemovedCallback(OnEmployeeRemoved);
	}

    void LoadSprites()
    {
        EmployeeSpriteMap = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Employees");

        foreach (Sprite s in sprites)
        {
            EmployeeSpriteMap[s.name] = s;
        }
    }
	
    void OnEmployeeCreated(Employee e)
    {
        GameObject EmpGo = new GameObject();
        EmployeeGameObjectMap.Add(e, EmpGo);
        EmpGo.name = "Employee_" + e.Name;

        EmpGo.transform.position = new Vector3(e.X + e.XModifier, e.Y + e.YModifier, 0);
        EmpGo.transform.SetParent(this.transform, true);
        EmpGo.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = EmpGo.GetComponent<SpriteRenderer>();
        EmpGo.AddComponent<Button>();
        EmpGo.GetComponent<Button>().onClick.AddListener(() => { e.cbEmployeeSelected(e); });
        sr.sortingLayerName = Words.Current.CharacterLayer;
        OnEmployeeChangedDirection(e);

        e.AddPersonPositionChangedCallback(OnEmployeeChangedPosition);
        e.AddPersonDirectionChangedCallback(OnEmployeeChangedDirection);

    }

    void OnEmployeeChangedPosition(Employee e)
    {
        if (EmployeeGameObjectMap.ContainsKey(e) == false)
        {
            return;
        }

        GameObject EmpGo = EmployeeGameObjectMap[e];

        EmpGo.transform.position = new Vector3(e.X + e.XModifier, e.Y + e.YModifier);
        string SpriteName = e.SpriteName + " " + e.Direction + " " + GetAnimationNumber(e).ToString();
        SpriteRenderer sr = EmpGo.GetComponent<SpriteRenderer>();
        sr.sprite = EmployeeSpriteMap[SpriteName];
        EmpGo.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        if (e.UserSelected)
        {
            sr.color = new Color(0.5f, 1f, 0.5f, 1f);
        }
        else { sr.color = new Color(1f, 1f, 1f, 1f); }
    }

    void OnEmployeeChangedDirection(Employee e)
    {
        if (EmployeeGameObjectMap.ContainsKey(e) == false)
        {
            return;
        }

        GameObject EmpGo = EmployeeGameObjectMap[e];
        string SpriteName = e.SpriteName + " " + e.Direction + " " + GetAnimationNumber(e).ToString();
        SpriteRenderer sr = EmpGo.GetComponent<SpriteRenderer>();
        sr.sprite = EmployeeSpriteMap[SpriteName];
        EmpGo.transform.localScale = new Vector3(0.5f, 0.5f, 1);


    }

    int GetAnimationNumber(Employee e)
    {

        if (e.NumFrames > e.Speed * 5)
        {
            if (e.Walking)
            {
                e.AnimationNumber++;
                e.NumFrames = 0;
                if (e.AnimationNumber == 4)
                {
                    e.AnimationNumber = 1;
                }
            }
            else { e.AnimationNumber = 1; }

            return e.AnimationNumber;
        }
        e.NumFrames++;
        return e.AnimationNumber;
    }

    void OnEmployeeRemoved(Employee e)
    {
        if (EmployeeGameObjectMap.ContainsKey(e))
        {
            GameObject EmpGo = EmployeeGameObjectMap[e];
            Destroy(EmpGo);
            EmployeeGameObjectMap.Remove(e);
        }
    }
}
