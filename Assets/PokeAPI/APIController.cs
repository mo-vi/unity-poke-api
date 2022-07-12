using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace PokeAPI
{
    public class APIController : MonoBehaviour
    {
        /* This is a summarized version of the real .json */
        /* There are .json processing libraries but this way you can access the data as a real .json inside Unity */
        [System.Serializable]
        public class Pokemon
        {
            public string name;
            public Sprites sprites;
            public Types[] types;

            [System.Serializable]
            public class Sprites
            {
                public string front_default;
            }

            [System.Serializable]
            public class Types
            {
                public Type type;

                [System.Serializable]
                public class Type
                {
                    public string name;
                }
            }
        }

        [SerializeField] RawImage pokeRawImage;
        [SerializeField] TextMeshProUGUI pokeName;
        [SerializeField] TextMeshProUGUI pokeNumber;
        [SerializeField] TextMeshProUGUI[] pokeTypes;
        [SerializeField] int existingPokemon;

        readonly string baseURL = "https://pokeapi.co/api/v2/pokemon/";

        void Start()
        {
            /* Show nothing until a request is fulfilled */
            pokeRawImage.texture = Texture2D.blackTexture;
            pokeName.text = null;
            pokeNumber.text = null;

            foreach (TextMeshProUGUI pokeType in pokeTypes)
            {
                pokeType.text = null;
            }
        }

        public void OnButtonRandomPokemon()
        {
            /* Maximum value is exclusive */
            int randomPokeIndex = Random.Range(1, existingPokemon + 1);

            /* Clear previous request and/or show a new Pokemon is being requested */
            pokeRawImage.texture = Texture2D.blackTexture;
            pokeName.text = "Loading...";
            pokeNumber.text = "#" + randomPokeIndex;

            foreach (TextMeshProUGUI pokeType in pokeTypes)
            {
                pokeType.text = "Loading...";
            }

            /* !!W Can be improved? */
            StartCoroutine(GetPokemonAtIndex(randomPokeIndex));
        }

        IEnumerator GetPokemonAtIndex(int pokeIndex)
        {
            var pokeURL = baseURL + pokeIndex.ToString();

            /* JSON request: Pokemon */
            var request = UnityWebRequest.Get(pokeURL);
            yield return request.SendWebRequest();

            /* Break the coroutine if the server returned an error */
            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                yield break;
            }

            var pokeJSON = JsonUtility.FromJson<Pokemon>(request.downloadHandler.text);

            pokeName.text = CapitalizeFirstLetter(pokeJSON.name);

            /* --- End of NAME processing --- */

            for (int i = 0; i < pokeJSON.types.Length; i++)
            {
                pokeTypes[i].text = CapitalizeFirstLetter(pokeJSON.types[i].type.name);

                /* By definition any Pokemon can have only two types. It's ok to declare it this way. */
                if (pokeJSON.types.Length == 1)
                {
                    /* The second type text will be printing 'Loading...' if not handled. */
                    pokeTypes[1].text = null;
                }
            }

            /* --- End of TYPES processing --- */

            var pokeSpriteURL = pokeJSON.sprites.front_default;

            /* JSON request: Sprite */
            var spriteRequest = UnityWebRequestTexture.GetTexture(pokeSpriteURL);
            yield return spriteRequest.SendWebRequest();

            /* Break the coroutine if the server returned an error */
            if (spriteRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(spriteRequest.error);
                yield break;
            }

            pokeRawImage.texture = DownloadHandlerTexture.GetContent(spriteRequest);
            pokeRawImage.texture.filterMode = FilterMode.Point;

            /* --- End of RAWIMAGE processing --- */
        }

        string CapitalizeFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}
