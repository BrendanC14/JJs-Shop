using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonSpriteController : MonoBehaviour {


    Dictionary<Person, GameObject> PersonGameObjectMap;
    Dictionary<string, Sprite> PersonSpriteMap;

	// Use this for initialization
	void Start () {
        LoadSprites();

        PersonGameObjectMap = new Dictionary<Person, GameObject>();
        World.Current.AddPersonCreatedCallback(OnPersonCreated);
        World.Current.AddPersonRemovedCallback(OnPersonRemoved);

	}
	

    void LoadSprites()
    {
        PersonSpriteMap = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/People");

        foreach (Sprite s in sprites)
        {
            PersonSpriteMap[s.name] = s;
        }
    }

    void OnPersonCreated(Person p)
    {
        GameObject persGO = new GameObject();
        PersonGameObjectMap.Add(p, persGO);
        persGO.name = "Person";

        persGO.transform.position = new Vector3(p.X + p.XModifier, p.Y + p.YModifier, 0);
        persGO.transform.SetParent(this.transform, true);
        persGO.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = persGO.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = Words.Current.CharacterLayer;
        OnPersonChangedDirection(p);

        p.AddPersonPositionChangedCallback(OnPersonChangedPosition);
        p.AddPersonDirectionChangedCallback(OnPersonChangedDirection);
    }

    void OnPersonChangedPosition(Person p)
    {
        if (PersonGameObjectMap.ContainsKey(p) == false)
        {
            return;
        }
        GameObject persGO = PersonGameObjectMap[p];

        persGO.transform.position = new Vector3(p.X + p.XModifier, p.Y + p.YModifier);
        string SpriteName = p.SpriteName + " " + p.Direction + " " + GetAnimationNumber(p).ToString();
        SpriteRenderer sr = persGO.GetComponent<SpriteRenderer>();
        sr.sprite = PersonSpriteMap[SpriteName];
        persGO.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        if (p.UserSelected)
        {
            sr.color = new Color(0.5f, 1f, 0.5f, 1f);
        }
        else { sr.color = new Color(1f, 1f, 1f, 1f); }
    }

    void OnPersonChangedDirection(Person p)
    {
        if (PersonGameObjectMap.ContainsKey(p) == false)
        {
            return;
        }

        GameObject persGO = PersonGameObjectMap[p];
        string SpriteName = p.SpriteName + " " + p.Direction + " " + GetAnimationNumber(p).ToString();
        SpriteRenderer sr = persGO.GetComponent<SpriteRenderer>();
        sr.sprite = PersonSpriteMap[SpriteName];
        persGO.transform.localScale = new Vector3(0.5f, 0.5f, 1);

    }

    int GetAnimationNumber(Person p)
    {

        if (p.NumFrames > p.Speed * 5)
        {
            if (p.Walking)
            {
                p.AnimationNumber++;
                p.NumFrames = 0;
                if (p.AnimationNumber == 4)
                {
                    p.AnimationNumber = 1;
                }
            }
            else { p.AnimationNumber = 1; }

            return p.AnimationNumber;
        }
        p.NumFrames++;
        return p.AnimationNumber;
    }

    void OnPersonRemoved(Person p)
    {
        if (PersonGameObjectMap.ContainsKey(p))
        {
            GameObject persGO = PersonGameObjectMap[p];
            Destroy(persGO);
            PersonGameObjectMap.Remove(p);
        }
    }
}
