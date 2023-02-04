using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostItem : MonoBehaviour
{
    public BoxCollider solidCollider; // definir manualmente

    public Renderer mRenderer;
    private Material semiTransparentMat; // Usado para debug - em vez de totalmente transparente
    private Material fullTransparentMat;
    private Material selectedMaterial;

    public bool isPlaced;

    // Uma flag para o algoritmo de remo��o
    public bool hasSamePosition = false;
    private void Start()
    {
        mRenderer = GetComponent<Renderer>();
        // Arranjamo-lo do gestor, porque assim a refer�ncia existe sempre
        semiTransparentMat = ConstructionManager.Instance.ghostSemiTransparentMat;
        fullTransparentMat = ConstructionManager.Instance.ghostFullTransparentMat;
        selectedMaterial = ConstructionManager.Instance.ghostSelectMat;

        mRenderer.material = fullTransparentMat; //mudar para semi para debug caso contr�rio muda-se para totalmente transparente
        // Desativamos a caixa de colis�o s�lda - enquanto ainda n�o est� colocado
        // (a n�o ser que estajamos no modo de constru��o - ver o m�todo de Update)
        solidCollider.enabled = false;
    }

    private void Update()
    {
        if (ConstructionManager.Instance.inConstructionMode)
        {
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), ConstructionManager.Instance.player.GetComponent<Collider>());
        }

        // Precisamos de uma colis�o s�lida para que o raycast a detete
        if (ConstructionManager.Instance.inConstructionMode && isPlaced)
        {
            solidCollider.enabled = true;
        }

        if (!ConstructionManager.Instance.inConstructionMode)
        {
            solidCollider.enabled = false;
        }

        // Ativar o material
        if (ConstructionManager.Instance.selectedGhost == gameObject)
        {
            mRenderer.material = selectedMaterial; //Verde
        }
        else
        {
            mRenderer.material = fullTransparentMat; //mudar para semi se for para debug caso contr�rio fica totalmente transparente
        }
    }
}
