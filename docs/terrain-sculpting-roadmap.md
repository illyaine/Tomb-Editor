# Terrain sculpting and sector subdivision roadmap

## Goal

Add a modern terrain sculpting workflow to Tomb Editor while keeping existing projects and classic 1024-unit sectors fully compatible by default.

The existing sector grid remains the authoritative legacy representation until each later phase is proven safe. Subdivision data is introduced as an optional layer and must not change existing levels when it is absent.

## Current architecture findings

- A room is made from fixed 1024-unit sectors.
- Each floor and ceiling sector surface stores four corner heights.
- Room geometry is rebuilt from those sector surfaces.
- The editor already has a Terrain tool and generates a Diamond-Square height map, but it applies the result only to legacy sector corners.
- Geometry undo already snapshots complete room sectors.
- PRJ2 loading and saving currently know only the legacy sector surface representation.
- Tomb Engine collision and floor queries must be reviewed separately before subdivided walkable geometry is enabled at runtime.

## Safety rules

1. Never change the meaning or size of a legacy sector globally.
2. Existing PRJ2 files must load exactly as before.
3. Existing projects must save without new data unless subdivision is explicitly enabled.
4. New subdivision data must be optional and versioned.
5. Rendering support comes before runtime collision support.
6. Runtime collision changes must be isolated to Tomb Engine and must keep legacy behavior as the default path.
7. Each phase must compile and be reviewable independently before the next phase starts.

## Planned phases

### Phase 1 - Data foundation

Add an isolated subdivided surface representation with power-of-two subdivision counts. It can initialize itself from an unsplit legacy sector surface without changing Sector, Room, PRJ2, rendering, or the compiler.

Initial supported subdivision counts: 2x2, 4x4, and 8x8 cells inside one legacy sector.

### Phase 2 - Editor-only preview

Allow a selected sector to generate a subdivided preview mesh in the 3D viewport. This phase remains non-persistent and does not affect compiled levels.

### Phase 3 - Persistent project data

Add an optional PRJ2 chunk for subdivided floor and ceiling data. Old projects remain unchanged. New data is written only when subdivision is enabled.

### Phase 4 - Sculpt brush

Replace selection-based terrain dragging with a brush workflow for subdivided surfaces.

Planned brush operations:

- Raise
- Lower
- Smooth
- Flatten
- Slope
- Noise

Brush radius, strength, falloff, and stroke undo must be supported.

### Phase 5 - Tomb Editor room mesh output

Teach RoomGeometry and the Tomb Engine compiler path to emit subdivided visual triangles while preserving legacy sector boundaries and portals.

### Phase 6 - Tomb Engine collision

Add an optional high-resolution floor and ceiling collision path for subdivided sectors. Legacy 1024-unit floor data remains available for compatibility.

### Phase 7 - Modern architecture tools

Build higher-level tools on the same subdivision layer:

- Edge and vertex selection
- Local face subdivision
- Bevel and chamfer helpers
- Curved slopes and arches
- Terrain-to-structure transitions
- Adaptive subdivision where extra detail is needed

## First implementation boundary

Phase 1 must not modify Sector.cs, Room.cs, RoomGeometry.cs, PRJ2 IO, compiler code, or Tomb Engine runtime code. This makes the first code change inert until explicitly integrated in a later phase.
