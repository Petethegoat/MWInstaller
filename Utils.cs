using System.Text;

public class Utils
{
    public static string RelativePath(string absolutePath, string relativeTo)
    {
        string[] absoluteDirectories = absolutePath.Split('\\');
        string[] relativeDirectories = relativeTo.Split('\\');

        //Get the shortest of the two paths
        int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

        //Use to determine where in the loop we exited
        int lastCommonRoot = -1;
        int index;

        //Find common root
        for(index = 0; index < length; index++)
            if(absoluteDirectories[index] == relativeDirectories[index])
                lastCommonRoot = index;
            else
                break;

        //If we didn't find a common prefix then throw
        if(lastCommonRoot == -1)
            throw new System.Exception("Paths do not have a common base");

        //Build up the relative path
        StringBuilder relativePath = new StringBuilder();

        //Add on the ..
        for(index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
            if(absoluteDirectories[index].Length > 0)
                relativePath.Append("..\\");

        //Add on the folders
        for(index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
            relativePath.Append(relativeDirectories[index] + "\\");
        relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

        return relativePath.ToString();
    }
}