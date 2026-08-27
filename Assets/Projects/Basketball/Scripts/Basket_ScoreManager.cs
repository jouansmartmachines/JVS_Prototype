using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basket
{
    public class Basket_ScoreManager : MonoBehaviour
    {
        [SerializeField] Basket_VFX vfx_1;
        [SerializeField] Basket_VFX vfx_2;
        [SerializeField] Basket_VFX vfx_3;

        public void AddScore(bool IsP1)
        {
            BasketTeam team = Basket_GameManager.i.Teams[IsP1 ? 1 : 0];
            team.Score += team.ScoreMultiplicator;
            team.ScoreText.text = $": {team.Score.ToString("00")}";
            //team.ScoreDisplay.DisplayScore(team.Score);
            team.Next = true;

            Basket_VFX vfx;
            switch (team.ScoreMultiplicator)
            {
                case 1:
                    vfx = vfx_1;
                    break;

                case 2:
                    vfx = vfx_2;
                    break;

                case 3:
                    vfx = vfx_3;
                    break;

                default:
                    vfx = vfx_1;
                    break;
            }
            vfx = Instantiate(vfx, team.Net.transform);
            vfx.Setup(team.Cam.transform);
            Destroy(vfx.gameObject, 5f);
        }
    }
}