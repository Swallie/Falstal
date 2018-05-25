using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileAutomator : MonoBehaviour {

    // Chance, dass eine Zelle ab Start initialisiert ist.
    [Range(0, 100)]
    public int iniChance;

    // Chance, das der Boden mit Deko versehen ist
    [Range(1, 30)]
    public int decoChance;

    [Range(0, 8)]
    public int birthLimit;

    [Range(0, 8)]
    public int deathLimit;

    // Wie viele durchläufe macht das Skript bei der generierung
    [Range(0, 8)]
    public int numR;

    // Interne Repräsentation der Map 
    public int[,] terrainMap;

    public Vector3Int tmapSize;

    // Passierbare
    public Tilemap botMap;
    public Tile botTile;

    // Nicht passierbare
    public Tilemap wallMap;
    public Tile blockingTile;
    public Tile voidTile;
    public Tile borderTop0;
    public Tile borderTop1;
    public Tile borderTop2;
    public Tile borderTop3;
    public Tile borderTopLeft;
    public Tile borderLeft;
    public Tile borderBottomLeft;
    public Tile borderBottom;
    public Tile borderBottomRight;
    public Tile borderRight;
    public Tile borderTopRight;

    // Dekoration
    public Tilemap decoMap;
    public Tile[] decoTiles;

    // Wie hoch/breit wird die Map?
    int width;
    int height;

    // Durchlauf
    public void doGenerateFloor(int numR)
    {
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

        doGenerateOuterWalls();

        // Dann machen wir so viele durchläufe wie gewünscht...
        for (int i = 0; i < numR; i++)
        {
            terrainMap = gentilePos(terrainMap);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (terrainMap[x, y] == 0)
                    botMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), botTile);
                else
                if (wallMap.GetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0)) == null)
                {
                    if (y > 0 && terrainMap[x, y-1] == 0)
                    {
                        wallMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), borderTop0);
                    } else
                    {
                        wallMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), blockingTile);
                    }
                }
            }
        }
    }

    public void doGenerateOuterWalls()
    {
        int borderThickness = 20;

        for (int x = - borderThickness; x <= width + borderThickness; x++)
        {
            for (int y = -borderThickness; y <= height + borderThickness; y++)
            {
                if (x >= 0 && y >= 0 && x < width && y < height)
                {
                    continue;
                }

                wallMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), blockingTile);
            }

        }



        // Oberkante und Unterkante
        for (int x = 0; x < width; x++)
        {
            // Oberkante
            wallMap.SetTile(new Vector3Int(-x + width / 2, 0 + height / 2, 0), borderTop0);
            wallMap.SetTile(new Vector3Int(-x + width / 2, 1 + height / 2, 0), borderTop1);
            wallMap.SetTile(new Vector3Int(-x + width / 2, 2 + height / 2, 0), borderTop2);
            wallMap.SetTile(new Vector3Int(-x + width / 2, 3 + height / 2, 0), borderTop3);

            // Unterkante
            wallMap.SetTile(new Vector3Int(-x + width / 2, -(height / 2), 0), borderBottom);
        }

        // Linke und Rechte Kante
        for (int y = 0; y < height; y++)
        {
            // Linke Kante
            wallMap.SetTile(new Vector3Int(-width / 2, -y + height / 2, 0), borderLeft);

            // Rechte Kante
            wallMap.SetTile(new Vector3Int(1 + width / 2, -y + height / 2, 0), borderRight);
        }

        // Die Ecken noch
        // Oben Rechts
        wallMap.SetTile(new Vector3Int(1 + width / 2, 3 + height / 2, 0), borderTopRight);
        wallMap.SetTile(new Vector3Int(1 + width / 2, 2 + height / 2, 0), borderRight);
        wallMap.SetTile(new Vector3Int(1 + width / 2, 1 + height / 2, 0), borderRight);

        // Unten Rechts
        wallMap.SetTile(new Vector3Int(1 + width / 2, - height / 2, 0), borderBottomRight);

        // Oben Links
        wallMap.SetTile(new Vector3Int(-width / 2, 3 + height / 2, 0), borderTopLeft);
        wallMap.SetTile(new Vector3Int(-width / 2, 2 + height / 2, 0), borderLeft);
        wallMap.SetTile(new Vector3Int(-width / 2, 1 + height / 2, 0), borderLeft);

        // Unten Links
        wallMap.SetTile(new Vector3Int(- width / 2, -height / 2, 0), borderBottomLeft);
    }

    // Wir verstreuen ein wenig Müll im Dungeon, damit es verlebter aussieht
    public void doDecorate()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] == 0 && Random.Range(1, 101) < decoChance)
                {
                    decoMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), decoTiles[Random.Range(0, decoTiles.Length)]);
                }
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
        decoMap.ClearAllTiles();

        if (complete)
        {
            terrainMap = null;
        }
    }


    void Start()
    {
        // Wir leeren die Karte
        clearMap(true);

        // Dann generieren wir den Boden
        doGenerateFloor(numR);

        // Deko muss hinein
        doDecorate();
        // Gegner

    }
}
