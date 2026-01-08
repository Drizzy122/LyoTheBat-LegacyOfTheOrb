//This script exists to not cause issue with users who are updating from older versions.
//This will be removed after a few months have passed.
//If you are reading this, feel free to remove this file.
//Added on: 16 June 2025

#if UNITY_EDITOR
namespace TinyGiantStudio.BetterInspector
{
    /// <summary>
    /// This is responsible for finding the correct ScriptableObject
    /// </summary>
    public static class ScalesFinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>The scales ScriptableObject file. This can be null</returns>
        public static ScalesManager MyScales()
        {
            return ScalesManager.instance;
        }

    }
}
#endif