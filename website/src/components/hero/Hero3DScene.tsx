import React, { useRef, useState, useEffect, Suspense } from 'react';
import { Canvas, useFrame, useThree } from '@react-three/fiber';
import { Float, Points, PointMaterial } from '@react-three/drei';
import * as THREE from 'three';

// 3D Normalization Core Engine Model
const NormalizationCore: React.FC<{ pointer: { x: number; y: number } }> = ({ pointer }) => {
  const outerRingRef = useRef<THREE.Mesh>(null!);
  const midRingRef = useRef<THREE.Mesh>(null!);
  const innerRingRef = useRef<THREE.Mesh>(null!);
  const coreIcosaRef = useRef<THREE.Mesh>(null!);
  const cageRef = useRef<THREE.LineSegments>(null!);
  const groupRef = useRef<THREE.Group>(null!);

  useFrame((state, delta) => {
    // Idle rotations with mathematical offsets
    if (outerRingRef.current) {
      outerRingRef.current.rotation.x += delta * 0.25;
      outerRingRef.current.rotation.y += delta * 0.15;
    }
    if (midRingRef.current) {
      midRingRef.current.rotation.y -= delta * 0.35;
      midRingRef.current.rotation.z += delta * 0.2;
    }
    if (innerRingRef.current) {
      innerRingRef.current.rotation.z += delta * 0.45;
      innerRingRef.current.rotation.x -= delta * 0.2;
    }
    if (coreIcosaRef.current) {
      coreIcosaRef.current.rotation.y += delta * 0.5;
      coreIcosaRef.current.rotation.x += delta * 0.3;
    }
    if (cageRef.current) {
      cageRef.current.rotation.y -= delta * 0.2;
      cageRef.current.rotation.z -= delta * 0.15;
    }

    // Pointer-responsive subtle parallax interpolation
    if (groupRef.current) {
      const targetX = pointer.x * 0.35;
      const targetY = pointer.y * 0.25;
      groupRef.current.rotation.y = THREE.MathUtils.lerp(groupRef.current.rotation.y, targetX, 0.05);
      groupRef.current.rotation.x = THREE.MathUtils.lerp(groupRef.current.rotation.x, -targetY, 0.05);
    }
  });

  return (
    <group ref={groupRef} scale={[1.1, 1.1, 1.1]}>
      {/* Outer Floating Concentric Torus Ring - Cyan Accent */}
      <mesh ref={outerRingRef}>
        <torusGeometry args={[2.5, 0.035, 32, 100]} />
        <meshStandardMaterial
          color="#06B6D4"
          emissive="#06B6D4"
          emissiveIntensity={0.6}
          roughness={0.1}
          metalness={0.9}
        />
      </mesh>

      {/* Middle Ring - Violet Accent */}
      <mesh ref={midRingRef}>
        <torusGeometry args={[1.9, 0.045, 32, 100]} />
        <meshStandardMaterial
          color="#8B5CF6"
          emissive="#8B5CF6"
          emissiveIntensity={0.7}
          roughness={0.15}
          metalness={0.85}
        />
      </mesh>

      {/* Inner Precision Ring - Bi-color highlight */}
      <mesh ref={innerRingRef}>
        <torusGeometry args={[1.35, 0.03, 32, 100]} />
        <meshStandardMaterial
          color="#38BDF8"
          emissive="#38BDF8"
          emissiveIntensity={0.5}
          roughness={0.2}
          metalness={0.9}
        />
      </mesh>

      {/* Transparent Glass-like Mechanical Normalization Core */}
      <mesh ref={coreIcosaRef}>
        <octahedronGeometry args={[0.85, 0]} />
        <meshPhysicalMaterial
          color="#C084FC"
          emissive="#7C3AED"
          emissiveIntensity={0.3}
          roughness={0.05}
          metalness={0.2}
          transmission={0.88} // Glass refraction
          thickness={1.2}
          transparent={true}
          opacity={0.85}
          ior={1.5}
        />
      </mesh>

      {/* Mechanical Wireframe Cage around Core */}
      <lineSegments ref={cageRef}>
        <edgesGeometry args={[new THREE.IcosahedronGeometry(1.05, 1)]} />
        <lineBasicMaterial color="#A78BFA" transparent opacity={0.4} />
      </lineSegments>

      {/* Geometric Gyro Satellite Nodes */}
      {[-1, 1].map((sign, i) => (
        <group key={i} position={[sign * 2.5, 0, 0]}>
          <mesh>
            <sphereGeometry args={[0.07, 16, 16]} />
            <meshStandardMaterial color="#06B6D4" emissive="#06B6D4" emissiveIntensity={1} />
          </mesh>
        </group>
      ))}
    </group>
  );
};

// Ambient Particle Dust
const CosmicParticles: React.FC = () => {
  const pointsRef = useRef<THREE.Points>(null!);
  const [coords] = useState(() => {
    const count = 140;
    const positions = new Float32Array(count * 3);
    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 12;
      positions[i * 3 + 1] = (Math.random() - 0.5) * 12;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 8;
    }
    return positions;
  });

  useFrame((_, delta) => {
    if (pointsRef.current) {
      pointsRef.current.rotation.y += delta * 0.03;
      pointsRef.current.rotation.x -= delta * 0.015;
    }
  });

  return (
    <points ref={pointsRef}>
      <bufferGeometry>
        <bufferAttribute
          attach="attributes-position"
          count={coords.length / 3}
          array={coords}
          itemSize={3}
        />
      </bufferGeometry>
      <pointsMaterial
        size={0.045}
        color="#8B5CF6"
        transparent
        opacity={0.6}
        blending={THREE.AdditiveBlending}
      />
    </points>
  );
};

export const Hero3DScene: React.FC = () => {
  const [pointer, setPointer] = useState({ x: 0, y: 0 });
  const [isReducedMotion, setIsReducedMotion] = useState(false);
  const [webGLSupported, setWebGLSupported] = useState(true);

  useEffect(() => {
    // Check reduced motion preference
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    setIsReducedMotion(mediaQuery.matches);
    const listener = (e: MediaQueryListEvent) => setIsReducedMotion(e.matches);
    mediaQuery.addEventListener('change', listener);

    // Basic WebGL detect
    try {
      const canvas = document.createElement('canvas');
      const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
      if (!gl) setWebGLSupported(false);
    } catch {
      setWebGLSupported(false);
    }

    const handleMouseMove = (e: MouseEvent) => {
      // Normalize pointer between -1 and 1
      const x = (e.clientX / window.innerWidth) * 2 - 1;
      const y = (e.clientY / window.innerHeight) * 2 - 1;
      setPointer({ x, y });
    };

    window.addEventListener('mousemove', handleMouseMove, { passive: true });
    return () => {
      mediaQuery.removeEventListener('change', listener);
      window.removeEventListener('mousemove', handleMouseMove);
    };
  }, []);

  // Graceful CSS Fallback for Reduced-Motion or Unsupported WebGL
  if (isReducedMotion || !webGLSupported) {
    return (
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none overflow-hidden">
        <div className="relative w-80 h-80 rounded-full border border-violet-500/20 flex items-center justify-center animate-pulse">
          <div className="w-64 h-64 rounded-full border border-cyan-500/30 flex items-center justify-center">
            <div className="w-48 h-48 rounded-full bg-gradient-to-tr from-violet-600/30 to-cyan-500/30 backdrop-blur-2xl border border-white/20 shadow-[0_0_50px_rgba(139,92,246,0.3)]" />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden">
      <Canvas
        camera={{ position: [0, 0, 6.2], fov: 45 }}
        gl={{ antialias: true, alpha: true, powerPreference: 'high-performance' }}
        dpr={[1, 1.5]}
      >
        <ambientLight intensity={0.4} />
        {/* Violet Key Light */}
        <pointLight position={[-4, 3, 3]} intensity={2.4} color="#8B5CF6" distance={12} />
        {/* Cyan Fill Light */}
        <pointLight position={[4, -3, 2]} intensity={2.6} color="#06B6D4" distance={12} />
        {/* Rim Light */}
        <directionalLight position={[0, 5, -3]} intensity={1.5} color="#C084FC" />

        <Suspense fallback={null}>
          <Float speed={1.8} rotationIntensity={0.2} floatIntensity={0.4}>
            <NormalizationCore pointer={pointer} />
          </Float>
          <CosmicParticles />
        </Suspense>
      </Canvas>
    </div>
  );
};
