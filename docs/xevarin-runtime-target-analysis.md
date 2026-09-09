# Xevarin runtime target analysis

## Goal

Evaluate whether the modern Tomb Editor geometry work should target TombEngine only, or whether Tomb Editor should also export directly to the Xevarin runtime pipeline.

## Current findings

### TombEngine

TombEngine separates rendered room geometry from gameplay floordata, but its gameplay floor and ceiling collision remains sector based.

- One gameplay sector is based on the classic 1024-unit block.
- A floor or ceiling surface contains exactly two collision triangles.
- Player floor and ceiling queries use FloorInfo and sector lookup.
- Creature pathfinding also resolves its pathfinding box through the sector grid.
- Rendered room geometry is more flexible and is loaded from vertex and polygon lists.

This means high-resolution subdivided geometry can be rendered by TEN, but fully matching high-resolution walkable collision would require a TEN runtime extension.

### Xevarin

Xevarin already has a runtime world model that is not restricted to classic Tomb Raider sectors.

- Runtime mesh resources store arbitrary vertices, indices, submeshes and materials.
- Runtime collision supports TriangleMesh, ConvexMesh and compound collision.
- Triangle-mesh collision stores arbitrary vertices and indices.
- Collision can independently define walkable, movement-blocking and camera-blocking behavior.
- Rooms and portals are represented separately from mesh topology.
- Xevarin already has a versioned .xevlevel container and LevelCooker pipeline.

Therefore Xevarin is a better natural target for true subdivided terrain, sculpted surfaces and architecture that is no longer constrained to two triangles per classic sector.

## Recommended architecture

Keep Tomb Editor authoring independent from the runtime target.

### Authoring layer

Tomb Editor owns the editable geometry representation:

- legacy 1024-unit sectors for compatibility;
- optional 2x2, 4x4 and 8x8 subdivision data;
- later free wall, edge and vertex editing;
- sculpt brush data;
- materials, rooms, objects, lights and portals.

### TombEngine target

Compile using the existing TEN pipeline for legacy-compatible projects.

Modern visual subdivisions may later be emitted as extra room geometry, but gameplay collision must either be approximated by legacy sectors or supported by a separate TEN runtime extension.

### Xevarin target

Do not implement a second .xevlevel writer inside Tomb Editor.

Instead:

1. Tomb Editor converts its authoring data to the supported Xevarin runtime-source schema.
2. Subdivided room surfaces become arbitrary indexed runtime meshes.
3. Walkable and blocking geometry becomes Xevarin TriangleMesh collision.
4. TE rooms become Xevarin runtime rooms.
5. TE portals become Xevarin runtime portals.
6. The existing Xevarin LevelCooker creates the final .xevlevel container.
7. The cooked file is read back and validated before publish.

This follows Xevarin's existing rule that .xevlevel container logic stays in LevelCore and tools call the shared cooker implementation.

## Why TEN should not be the permanent intermediate format

Using TE -> TEN -> Xevarin is useful as a compatibility test because Xevarin can currently discover legacy .ten levels.

However it should not be the final modern geometry path because the TEN floordata representation would discard or approximate the extra subdivision detail before Xevarin sees it.

The preferred final paths are therefore:

- TE -> TEN for TombEngine compatibility.
- TE -> Xevarin runtime source -> Xevarin LevelCooker -> .xevlevel for unrestricted modern geometry.

## Safe validation order

1. Keep the current subdivision work editor-only.
2. Build a 4x4 subdivided single-sector test mesh.
3. Export that mesh as Xevarin runtimeGeometry without changing gameplay systems.
4. Export matching TriangleMesh runtimeCollision.
5. Cook through the existing Xevarin LevelCooker.
6. Verify the cooked .xevlevel loads and renders.
7. Verify player movement follows the high-resolution collision rather than a legacy sector approximation.
8. Add a second test containing a wall pushed inward or outward from the original square boundary.
9. Only after those tests succeed, connect persistent TE subdivision data to the Xevarin exporter.

## Safety rule

The TEN and Xevarin targets must remain separate export backends. No modern authoring feature should silently degrade when the user targets Xevarin, and no existing TEN project should change behavior merely because the modern geometry modules exist.
