using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileAutomator : MonoBehaviour {

    // Chance, dass eine Zelle ab Start initialisiert ist.
    [Range(0, 100)]
    public int iniChance;

    [Range(0, 8)]
    public int birthLimit;

    [Range(0, 8)]
    public int deathLimit;

    // Wie viele durchläufe macht das Skript bei der generierung
    [Range(0, 8)]
    public int numR;

    private int count = 0;

    public int[,] terrainMap;

    public Vector3Int tmapSize;

    // Bodenfliesen
    public Tilemap botMap;
    public Tile botTile;

    // Wände
    public Tilemap wallMap;
    public Tile wallTile;

    // Wie hoch/breit wird die Map?
    int width;
    int height;

    // Durchlauf
    public void doSim(int numR)
    {
        // Karte leeren
        clearMap(false);

        // wir setzen höhe und breite
        width = tmapSize.x;
        height = tmapSize.y;

        // Wenn wir zum ersten Mal starten, generieren wir eine Map...
        if (terrainMap == null)
        {
            terrainMap = new int[width, height];

            // ...und initialisieren diese
            initPos();
        }

        // Dann machen wir so viele durchläufe wie gewünscht...
        for (int i = 0; i < numR; i++)
        {
            terrainMap = gentilePos(terrainMap);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] == 0)
                    botMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), botTile);
                else
                    wallMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), wallTile);
            }

        }
    }

    // Wir errechnen einen durchlauf (Conway's Game of life)
    public int [,] gentilePos(int[,] oldMap)
    {
        // Wir generieren eine neue Map, die wir zurückgeben können, sodass wir die Alte zur Berechnung nutzen können.
        int[,] newMap = new int[width, height];

        // Anzahl der lebenden Nachbarzellen
        int neighb;

        // Wir bilden ein Quadrath von 3x3 Feldern (das hilft uns gleich bei der Betrachtung der Umgebung)
        BoundsInt myB = new BoundsInt(-1, -1, 0, 3, 3, 1);

        // welcher Punkt wird aktuell betrachtet?
        int curX;
        int curY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                neighb = 0;

                foreach (var b in myB.allPositionsWithin)
                {
                    // Wir schließen das aktuelle Feld aus (0, 0)
                    if (b.x == 0 && b.y == 0) continue;

                    // welche Zelle betrachen wir gerade?
                    curX = x + b.x;
                    curY = y + b.y;

                    // Wir erhöhen immer um 1, da wir die Grenze als lebende Zelle betrachten wollen
                    neighb++;

                    // Liegen wir links ausserhalb der Map?
                    if (curX < 0) continue;

                    // Liegen wir rechts ausserhalb der Map?
                    if (curX >= width) continue;

                    // Liegen wir unten ausserhalb der Map?
                    if (curY < 0) continue;

                    // Liegen wir oben ausserhalb der Map?
                    if (curY >= height) continue;

                    // Wenn wir bis hier kommen, dann sind wir nicht am Rand...
                    neighb--;

                    // Dann schauen wir, ob der Nachbar lebt...
                    neighb += oldMap[curX, curY];
                }

                // Lebte die alte Zelle?
                if (oldMap[x, y] == 1)
                {
                    // Liegt die Anzahl der Nachbarn unter dem deathLimit, dann stirbt die Zelle, ansonsten lebt sie weiter
                    if (neighb < deathLimit) newMap[x, y] = 0;
                    else newMap[x, y] = 1;
                } else
                {
                    // Liegt die Anzahl der Nachbarn über dem birthLimit, dann wird eine Zelle geboren, ansonsten bleibt sie tot
                    if (neighb > birthLimit) newMap[x, y] = 1;
                    else newMap[x, y] = 0;
                }
            }

        }

        // Wir geben die neue Map zurück
        return newMap;
    }
        
    // Initialisierung einer zufälligen Karte
    public void initPos()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = Random.Range(1, 101) < iniChance ? 1 : 0;
            }
        }
    }

    // Wir clearen die Map
    public void clearMap(bool complete)
    {
        wallMap.ClearAllTiles();
        botMap.ClearAllTiles();

        if (complete)
        {
            terrainMap = null;
        }
    }


    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Generating");
            doSim(numR);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Clearing");
            clearMap(true);
        }
    }
}
