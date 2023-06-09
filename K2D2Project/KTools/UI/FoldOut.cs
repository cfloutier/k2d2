
using UnityEngine;

namespace KTools.UI;

public class FoldOut
{
    public delegate void onChapterUI();


    public class Chapter
    {
        public string Title;
        public onChapterUI chapterUI;
        public bool opened = false;

        public Chapter(string Title, onChapterUI chapterUI)
        {
            this.Title = Title;
            this.chapterUI = chapterUI;
        }
    }

    public List<Chapter> chapters = new List<Chapter>();
    public bool singleChapter = false;

    public void OnGui()
    {
        GUILayout.BeginVertical();

        for (int i = 0; i < chapters.Count; i++)
        {
            Chapter chapter = chapters[i];
            var style = chapter.opened ? KBaseStyle.foldout_open : KBaseStyle.foldout_close;
            if (GUILayout.Button(chapter.Title, style))
            {
                chapter.opened = !chapter.opened;


                if (chapter.opened && singleChapter)
                {
                    for (int j = 0; j < chapters.Count; j++)
                    {
                        if (i != j)
                            chapters[j].opened = false;
                    }
                }
            }

            if (chapter.opened)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();

                chapter.chapterUI();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

        }
        GUILayout.EndVertical();
    }

    public void addChapter(string Title, onChapterUI chapterUI)
    {
        chapters.Add(new Chapter(Title, chapterUI));
    }


    public int Count
    {
        get { return chapters.Count; }
    }

}
