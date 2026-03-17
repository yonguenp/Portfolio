using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ProducesRecipe
    {
        public int RecipeID { get; private set; } = -1;
        public int ProductionExp { get; private set; } = -1;
        public eProducesState State { get; private set; } = eProducesState.None;

        public ProducesRecipe(int id, int exp, eProducesState state = eProducesState.None)
        {
            SetData(id, exp, state);
        }

        public void SetData(int id, int exp, eProducesState state = eProducesState.None)
        {
            RecipeID = id;
            ProductionExp = exp;
            State = state;
        }
    }
}