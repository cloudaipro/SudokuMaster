using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NoteButton : Selectable, IPointerClickHandler
{
    public static NoteButton Instance;
    public Sprite OnImage;
    public Sprite OffImage;
    public GameObject OnBadge;
    public bool Active {get; set;} = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleActive();
    }

    public void ToggleActive()
    {
        Active = !Active;
        OnBadge.SetActive(Active);
        //GetComponent<Image>().sprite = (Active) ? on_image : off_image;
        GameEvents.OnNotesActiveMethod(Active);
        //PencilImage = Active ? OnImage : OffImage ;
        this.image.sprite = Active ? OnImage : OffImage;
    }
    protected override void Awake()
    { 
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Active = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
