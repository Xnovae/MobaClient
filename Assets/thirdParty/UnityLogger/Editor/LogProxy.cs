using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class LogProxy
{
	// the background texture2d's used for colour scheme
	[SerializeField] public Texture2D evenBackground;	
	[SerializeField] public Texture2D oddBackground;
	[SerializeField] public Texture2D pauseBackground;
	[SerializeField] public Texture2D selectedBackground;
	
    public void Init()
    {
        // check if we are setting for pro or basic unity3d
        if (EditorGUIUtility.isProSkin)
        {
            // apparently unity3d 3.x vs 4.x adjusts colours differently, treat 3.x differently
            evenBackground = (Texture2D)Resources.Load("texture_evenrow_dark_3");
            oddBackground = (Texture2D)Resources.Load("texture_oddrow_dark_3");
            selectedBackground = (Texture2D)Resources.Load("texture_selected_dark_3");

            /*
			if( Application.unityVersion.StartsWith( "3" ) ) 
			{

			}
			else
			{
				evenBackground = ( Texture2D )Resources.Load( "texture_evenrow_dark" );
				oddBackground = ( Texture2D )Resources.Load( "texture_oddrow_dark" );
				selectedBackground = ( Texture2D )Resources.Load( "texture_selected_dark" );
			}
			*/
        }
        else
        {
            // apparently unity3d 3.x vs 4.x adjusts colours differently, treat 3.x differently
            if (Application.unityVersion.StartsWith("3"))
            {
                evenBackground = (Texture2D)Resources.Load("texture_evenrow_light_3");
                oddBackground = (Texture2D)Resources.Load("texture_oddrow_light_3");
                selectedBackground = (Texture2D)Resources.Load("texture_selected_light_3");
            }
            else
            {
                evenBackground = (Texture2D)Resources.Load("texture_evenrow_light");
                oddBackground = (Texture2D)Resources.Load("texture_oddrow_light");
                selectedBackground = (Texture2D)Resources.Load("texture_selected_light");
            }
        }

        pauseBackground = (Texture2D)Resources.Load("texture_pause");
    }
	public LogProxy( )
	{
		
	}
}

