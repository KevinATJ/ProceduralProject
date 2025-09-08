# Generación Procedural de Terrenos en Unity

## Creado por Kevin Troncoso y José Peña

## 1) Detalles del proyecto
Este proyecto implementa un sistema de generación procedural de terrenos en Unity.  
El objetivo de este es la posibilidad de generar dos mapas:
  1) Mapa normal: un mapa de carácter natural, que combina agua, playa, vegetación y montañas nevadas.
  2) Mapa volcánico: un mapa de carácter volcánico, que incluye terrenos de grandes montañas de lava y piedra volcánica.

- **Motor:** Unity 3D 2022.3.30f1
- **Lenguaje:** C#  
---

## 2) Algoritmos usados
1. **Diamond-Square (DS):**  
   Es el encargado de la generación del mapa de altura. 

2. **Cellular Automata (CA):**  
   Es el encargado de suavizar las transiciones entre arena, agua y tierra.

3. **L-System:**  
   Es el encargado de la generación de 3 tipos de árboles.

---

## 3) Instrucciones de uso en Unity
1. Clona el repositorio:  [Repositorio] (https://github.com/KevinATJ/ProceduralProject)
2. Descarga o ejecuta el proyecto en Unity 2022.3.30f1
3. Dirigete a la escena DSQ_PCG
4. Revisa Terrain, Tree y Manager para realizar cambios si lo deseas.
5. Ejecuta el proyecto.
6. Cambia y regenera mapas con los botones de tu teclado [1 y 2] y muevete en el mundo con [WASD Q E]

---

## 4) Instrucciones de uso en Ejecutable
1. Ejecuta My Project
2. Cambia y regenera mapas con los botones de tu teclado [1 y 2] y muevete en el mundo con [WASD Q E]
3. Presiona tecla [ESC] para modificar los algoritmos y el mapa.


