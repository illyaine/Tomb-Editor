-- !Name "Particle generator (moveables)"
-- !Section "Particles"
-- !Description "Emit particles from a moveable"
-- !Arguments "NewLine, Moveables, 70, The moveable particles will spawn from." "Numerical, 15, [ 0 | 100], Mesh number"
-- !Arguments "Numerical, 15, [ 0 | 100 | 0 ], Sprite number. Refers to a DEFAULT_SPRITES sequence in a wad."
-- !Arguments "NewLine, Vector3, 60, [ -32000 | 32000 ], Velocity X Y Z"
-- !Arguments "Numerical, 20, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 20, [ -32000 | 32000 | 1 ], Rotation"
-- !Arguments "NewLine, Color, 33, Start color", "Color, 34, End color"
-- !Arguments "Enumeration, 33, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], Blending method for particles. \nSee Lua API Documentation for further information."
-- !Arguments "NewLine, Numerical, 20, [ -32000 | 32000 | 0 ], Start size" "Numerical, 20, [ -32000 | 32000 | 0 ], End size" "Numerical, 20, [ 0 | 32000 | 1 | .1 ], Lifetime (in seconds)"
-- !Arguments "Boolean, 20, Poison" "Boolean, 20, Damage"

LevelFuncs.Engine.Node.ParticleEmitter = function(entity, meshnum, spriteID, velocity, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, poison, damage)
	local origin = TEN.Objects.GetMoveableByName(entity):GetJointPosition(meshnum)
	local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

	TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode, startSize, endSize, life, damage, poison)
end

-- !Name "Particle generator (statics)"
-- !Section "Particles"
-- !Description "Emit particles from a moveable"
-- !Arguments "NewLine, Statics, 85, The moveable particles will spawn from."
-- !Arguments "Numerical, 15, [ 0 | 100 | 0 ], Sprite number. Refers to a DEFAULT_SPRITES sequence in a wad."
-- !Arguments "NewLine, Vector3, 60, [ -32000 | 32000 ], Velocity X Y Z"
-- !Arguments "Numerical, 20, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 20, [ -32000 | 32000 | 1 ], Rotation"
-- !Arguments "NewLine, Color, 6, Start color", "Color, 6, End color"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], Blending method for particles. \nSee Lua API Documentation for further information."
-- !Arguments "Numerical, 18.2, [ -32000 | 32000 | 0 ], Start size" "Numerical, 18.2, [ -32000 | 32000 | 0 ], End size" "Numerical, 24, [ 0 | 32000 | 1 | 0.1 ], Lifetime (in seconds)"
-- !Arguments "NewLine, Boolean, 15, Poison" "Boolean, 17, Damage"
-- !Arguments "Boolean, 65, Show particle only if static mesh is visible"

LevelFuncs.Engine.Node.ParticleEmitterStatics = function(entity, spriteID, velocity, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, poison, damage,visibility)
	local origin = TEN.Objects.GetStaticByName(entity):GetPosition()
	local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

	local shouldEmit = not visibility or LevelFuncs.Engine.Node.TestStaticActivity(entity)
	if shouldEmit then
		TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode,
			startSize, endSize, life, damage, poison)
	end
end

-- !Name "Particle generator (volume)"
-- !Section "Particles"
-- !Description "Emit particles from the centre of a volume"
-- !Arguments "NewLine, Volumes, 70, The volume particles will spawn from." "Numerical, 30, [ 0 | 100 | 0 ], Sprite number. Refers to a DEFAULT_SPRITES sequence in a wad."
-- !Arguments "NewLine, Vector3, 60, [ -32000 | 32000 ], Velocity X Y Z"
-- !Arguments "Numerical, 20, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 20, [ -32000 | 32000 | 1 ], Rotation"
-- !Arguments "NewLine, Color, 33, Start color", "Color, 34, End color"
-- !Arguments "Enumeration, 33, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], Blending method for particles. \nSee Lua API Documentation for further information."
-- !Arguments "NewLine, Numerical, 20, [ -32000 | 32000 | 0 ], Start size" "Numerical, 20, [ -32000 | 32000 | 0 ], End size" "Numerical, 20, [ 0 | 32000 | 1 | .1 ], Lifetime (in seconds)"
-- !Arguments "Boolean, 20, Poison" "Boolean, 20, Damage"

LevelFuncs.Engine.Node.ParticleEmitterVolume = function(volume, spriteID, velocity, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, poison, damage)
	local origin = TEN.Objects.GetVolumeByName(volume):GetPosition()
	local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

	TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode, startSize, endSize, life, damage, poison)
end

-- !Name "Emit lightning arc"
-- !Section "Particles"
-- !Description "Emit a lightning arc between two points in 3D space"
-- !Arguments "NewLine, Moveables, 100, Source position" 
-- !Arguments "NewLine, Moveables, 100, Destination position"
-- !Arguments "NewLine, Color, 20, Color of lightning Effect"
-- !Arguments "Numerical, 20, [ 0 | 4.2 | 1 ], Lifetime in seconds" "Numerical, 20, [ 1 | 255 | 0 ], Effect strength"
-- !Arguments "Numerical, 20, [ 1 | 127 | 0 ], Beam width" "Numerical, 20, [ 1 | 127 | 0 ], Detail level"
-- !Arguments "NewLine, Boolean, 22, Smooth" "Boolean, 22, End drift" "Boolean, 26, Source light" "Boolean, 30, Destination light"

LevelFuncs.Engine.Node.LightningArc = function(source, dest, color, lifetime, amplitude, beamWidth, detail, smooth, endDrift, sourcelight, destlight)
	local randomiserX = (math.random(-64, 64))
	local randomiserZ = (math.random(-256, 256))

	local entity = TEN.Objects.GetMoveableByName(source)

	local startingpoint = entity:GetPosition()

	startingpoint.x = (startingpoint.x - randomiserX)
	startingpoint.y = (startingpoint.y - 64)
	startingpoint.z = (startingpoint.z - randomiserZ)

	local endingpoint = TEN.Objects.GetMoveableByName(dest):GetPosition()

	endingpoint.x = (endingpoint.x - randomiserX)
	endingpoint.y = (endingpoint.y - 64)
	endingpoint.z = (endingpoint.z - randomiserZ)

	local beamRandom = math.random((beamWidth - 10), (beamWidth + 10))
	local ampRandom = math.random((amplitude - 10), (amplitude + 10))

	if (sourcelight == true) then
		TEN.Effects.EmitLight(startingpoint, color, math.random(1, 10))
	end

	if (destlight == true) then
		TEN.Effects.EmitLight(endingpoint, color, math.random(1, 10))
	end

	TEN.Effects.EmitLightningArc(startingpoint, endingpoint, color, lifetime, ampRandom, beamRandom, detail, smooth, endDrift)
end

-- !Name "Emit shockwave"
-- !Section "Particles"
-- !Description "Emit a shockwave effect."
-- !Arguments "NewLine, Moveables, 70, Shockwave position." "Numerical, 30, [ 0 | 100 ], Mesh number (optional)"
-- !Arguments "NewLine, Numerical, 33, [ 1 | 10400 | 0 ], Inner radius" "Numerical, 33, [ 1 | 10400 | 0 ], Outer radius"
-- !Arguments "Color, 34, Color of shockwave"
-- !Arguments "NewLine, Numerical, 33, [ 0 | 8.5 | 1 ], Lifetime of effect (in seconds)" "Numerical, 33, [ 0 | 500 | 0 ], Speed" "Numerical, 33, [ -360 | 360 | 0 ], X axis rotation"
-- !Arguments "NewLine, Boolean, 50, Damage" "Boolean, 50, Randomize spawn point"

LevelFuncs.Engine.Node.Shockwave = function(pos, meshnum, innerRadius, outerRadius, color, lifetime, speed, angle, damage,
											randomSpawn)
	local randomiser = math.random(1, 14)
	local radiusInVar = innerRadius + math.random(1, 3)
	local radiusOutVar = outerRadius + math.random(1, 3)
	local randomMesh = math.random(0, 14)

	if (randomSpawn == true) then
		local origin = TEN.Objects.GetMoveableByName(pos):GetJointPosition(randomMesh)
		TEN.Effects.EmitShockwave(origin, radiusInVar, radiusOutVar, color, lifetime, speed, angle, damage)
	else
		local origin = TEN.Objects.GetMoveableByName(pos):GetJointPosition(meshnum)
		TEN.Effects.EmitShockwave(origin, radiusInVar, radiusOutVar, color, lifetime, speed, angle, damage)
	end
end

-- !Name "Effect Box particle emitter"
-- !Section "Effect Box / Particles"
-- !Description "Emits advanced TEN particles from the Effect Box without requiring level Lua code. The emitter supports volume and surface distribution, burst counts, random intervals and property ranges."
-- !Arguments "NewLine, Boolean, 100, {true}, Enabled"
-- !Arguments "NewLine, Enumeration, 100, [ Centre | Inside volume | Top face | Bottom face | Side faces | Entire surface ], Spawn distribution"
-- !Arguments "NewLine, Numerical, 33, [ 1 | 256 | 0 | 1 | 10 ], {1}, Particles per burst" "Numerical, 33, [ 0 | 60 | 2 | 0.05 | 0.5 ], {0.25}, Minimum interval in seconds" "Numerical, 34, [ 0 | 60 | 2 | 0.05 | 0.5 ], {0.25}, Maximum interval in seconds"
-- !Arguments "NewLine, SpriteSlots, 65, Sprite sequence" "Numerical, 35, [ 0 | 255 | 0 ], {0}, Sprite frame"
-- !Arguments "NewLine, Vector3, 50, [ -32000 | 32000 | 2 | 1 | 10 ], {TEN.Vec3(0, 0, 0)}, Base velocity" "Vector3, 50, [ 0 | 32000 | 2 | 1 | 10 ], {TEN.Vec3(0, 0, 0)}, Random velocity spread"
-- !Arguments "NewLine, Boolean, 100, {true}, Rotate velocity with Effect Box"
-- !Arguments "NewLine, Numerical, 33, [ -32768 | 32767 | 2 | 1 | 10 ], {0}, Gravity" "Numerical, 33, [ 0 | 32767 | 2 | 1 | 10 ], {0}, Friction" "Numerical, 34, [ -32768 | 32767 | 2 | 1 | 10 ], {0}, Maximum Y velocity"
-- !Arguments "NewLine, Color, 50, {TEN.Color(255, 255, 255)}, Start color" "Color, 50, {TEN.Color(255, 255, 255)}, End color"
-- !Arguments "NewLine, Enumeration, 100, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], {7}, Blending method"
-- !Arguments "NewLine, Numerical, 33, [ 0 | 32000 | 2 | 1 | 10 ], {10}, Start size" "Numerical, 33, [ 0 | 32000 | 2 | 1 | 10 ], {0}, End size" "Numerical, 34, [ 0.1 | 120 | 2 | 0.1 | 1 ], {2}, Minimum lifetime"
-- !Arguments "NewLine, Numerical, 34, [ 0.1 | 120 | 2 | 0.1 | 1 ], {2}, Maximum lifetime" "Numerical, 33, [ -360 | 360 | 1 | 1 | 15 ], {0}, Start rotation" "Numerical, 33, [ -3600 | 3600 | 1 | 1 | 15 ], {0}, Rotation velocity"
-- !Arguments "NewLine, Boolean, 25, {false}, Wind" "Boolean, 25, {false}, Damage" "Boolean, 25, {false}, Poison" "Boolean, 25, {false}, Burn"
-- !Arguments "NewLine, Numerical, 33, [ 0 | 10000 | 0 | 1 | 10 ], {2}, Damage amount" "Boolean, 33, {false}, Animated sprite" "Numerical, 34, [ 0.01 | 120 | 2 | 0.05 | 1 ], {1}, Animation frame rate"
-- !Arguments "NewLine, Enumeration, 100, [ Loop | One shot | Back and forth | Lifetime spread ], {0}, Animation type"
-- !Arguments "NewLine, Boolean, 33, {false}, Particle light" "Numerical, 33, [ 0 | 128 | 0 | 1 | 4 ], {0}, Light radius in quarter blocks" "Numerical, 34, [ 0 | 255 | 0 | 1 | 10 ], {0}, Light flicker"

LevelFuncs.Engine.Node.EffectBoxParticleEmitter = function(enabled, distribution, burstCount, intervalMin, intervalMax,
	spriteSeqID, spriteID, velocity, velocitySpread, rotateVelocity, gravity, friction, maxYVel,
	startColor, endColor, blendID, startSize, endSize, lifeMin, lifeMax, startRot, rotVel,
	wind, damage, poison, burn, damageHit, animated, frameRate, animType, light, lightRadius, lightFlicker)
	-- This authoring function is replaced with the private runtime variant by Tomb Editor during level compilation.
end

local EFFECT_BOX_FPS = 30

local function EffectBoxRandomRange(minimum, maximum)
	minimum = minimum or 0
	maximum = maximum or minimum
	if maximum < minimum then
		minimum, maximum = maximum, minimum
	end
	return minimum + math.random() * (maximum - minimum)
end

local function EffectBoxRandomSigned(spread)
	spread = math.abs(spread or 0)
	return EffectBoxRandomRange(-spread, spread)
end

local function EffectBoxRandomLocalPosition(extents, distribution)
	if distribution == 0 then
		return TEN.Vec3(0, 0, 0)
	end

	local x = EffectBoxRandomRange(-extents.x, extents.x)
	local y = EffectBoxRandomRange(-extents.y, extents.y)
	local z = EffectBoxRandomRange(-extents.z, extents.z)

	if distribution == 2 then
		y = -extents.y
	elseif distribution == 3 then
		y = extents.y
	elseif distribution == 4 then
		local face = math.random(1, 4)
		if face == 1 then
			x = -extents.x
		elseif face == 2 then
			x = extents.x
		elseif face == 3 then
			z = -extents.z
		else
			z = extents.z
		end
	elseif distribution == 5 then
		local face = math.random(1, 6)
		if face == 1 then
			x = -extents.x
		elseif face == 2 then
			x = extents.x
		elseif face == 3 then
			y = -extents.y
		elseif face == 4 then
			y = extents.y
		elseif face == 5 then
			z = -extents.z
		else
			z = extents.z
		end
	end

	return TEN.Vec3(x, y, z)
end

LevelFuncs.Engine.Node.__EffectBoxParticleEmitterRuntime = function(volumeName, emitterKey, enabled, distribution,
	burstCount, intervalMin, intervalMax, spriteSeqID, spriteID, velocity, velocitySpread, rotateVelocity,
	gravity, friction, maxYVel, startColor, endColor, blendID, startSize, endSize, lifeMin, lifeMax,
	startRot, rotVel, wind, damage, poison, burn, damageHit, animated, frameRate, animType,
	light, lightRadius, lightFlicker)
	if not enabled then
		return
	end

	local volume = TEN.Objects.GetVolumeByName(volumeName)
	if not volume or not volume:GetActive() then
		return
	end

	LevelVars.Engine.EffectBoxEmitters = LevelVars.Engine.EffectBoxEmitters or {}
	local state = LevelVars.Engine.EffectBoxEmitters[emitterKey]
	if not state then
		state = { frames = 0 }
		LevelVars.Engine.EffectBoxEmitters[emitterKey] = state
	end

	state.frames = math.max(0, (state.frames or 0) - 1)
	if state.frames > 0 then
		return
	end

	local nextInterval = EffectBoxRandomRange(math.max(0, intervalMin or 0), math.max(0, intervalMax or intervalMin or 0))
	state.frames = math.max(1, math.floor(nextInterval * EFFECT_BOX_FPS + 0.5))

	local centre = volume:GetPosition()
	local rotation = volume:GetRotation()
	local extents = volume:GetScale()
	local blendMode = LevelFuncs.Engine.Node.GetBlendMode(blendID)
	local animationTypes =
	{
		TEN.Effects.ParticleAnimationType.LOOP,
		TEN.Effects.ParticleAnimationType.ONE_SHOT,
		TEN.Effects.ParticleAnimationType.BACK_AND_FORTH,
		TEN.Effects.ParticleAnimationType.LIFE_TIME_SPREAD
	}

	for index = 1, math.max(1, math.floor(burstCount or 1)) do
		local localPosition = EffectBoxRandomLocalPosition(extents, distribution or 0)
		local position = centre + localPosition:Rotate(rotation)
		local particleVelocity = velocity + TEN.Vec3(
			EffectBoxRandomSigned(velocitySpread.x),
			EffectBoxRandomSigned(velocitySpread.y),
			EffectBoxRandomSigned(velocitySpread.z))

		if rotateVelocity then
			particleVelocity = particleVelocity:Rotate(rotation)
		end

		local particle =
		{
			pos = position,
			vel = particleVelocity,
			spriteSeqID = spriteSeqID,
			spriteID = spriteID,
			life = EffectBoxRandomRange(math.max(0.1, lifeMin or 0.1), math.max(0.1, lifeMax or lifeMin or 0.1)),
			maxYVel = maxYVel,
			gravity = gravity,
			friction = friction,
			startRot = startRot,
			rotVel = rotVel,
			startSize = startSize,
			endSize = endSize,
			startColor = startColor,
			endColor = endColor,
			blendMode = blendMode,
			wind = wind,
			damage = damage,
			poison = poison,
			burn = burn,
			damageHit = damageHit,
			animated = animated,
			frameRate = frameRate,
			animType = animationTypes[(animType or 0) + 1] or TEN.Effects.ParticleAnimationType.LOOP,
			light = light,
			lightRadius = lightRadius,
			lightFlicker = lightFlicker
		}

		TEN.Effects.EmitAdvancedParticle(particle)
	end
end
