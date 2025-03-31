const socket = new WebSocket('ws://localhost:1738');

// Storage for received data
const ledData = {
    positions: {},
    colors: {}
};

socket.onmessage = (event) => {
    const message = JSON.parse(event.data);

    // Handle different message types
    if (message.type === "positions") {
        message.data.forEach(item => {
            ledData.positions[item.id] = item.data;
        });
        renderScene();
    }
    else if (message.type === "colors") {
        message.data.forEach(item => {
            ledData.colors[item.id] = item.data;
        });
        renderScene();
    }
};

const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
const renderer = new THREE.WebGLRenderer();

renderer.setSize(window.innerWidth, window.innerHeight);
document.body.appendChild(renderer.domElement);

function HSVtoRGB(h, s, v) {
    let c = v * s;
    let x = c * (1 - Math.abs((h / 60) % 2 - 1));
    let m = v - c;
    let r, g, b;
    if (h >= 0 && h < 60) [r, g, b] = [c, x, 0];
    else if (h >= 60 && h < 120) [r, g, b] = [x, c, 0];
    else if (h >= 120 && h < 180) [r, g, b] = [0, c, x];
    else if (h >= 180 && h < 240) [r, g, b] = [0, x, c];
    else if (h >= 240 && h < 300) [r, g, b] = [x, 0, c];
    else [r, g, b] = [c, 0, x];

    return [(r + m) * 255, (g + m) * 255, (b + m) * 255];
}

function renderScene() {
    // Clear existing objects
    while (scene.children.length > 0) {
        if (scene.children[0].isLight) {
            break; // Skip lights
        }
        scene.remove(scene.children[0]);
    }

    // For each strip ID that has both position and color data
    Object.keys(ledData.positions).forEach(id => {
        const positions = ledData.positions[id];
        const colors = ledData.colors[id];

        // Only render if we have both position and color data for this ID
        if (positions && colors && positions.length === colors.length) {
            for (let i = 0; i < positions.length; i++) {
                const position = positions[i];
                const color = colors[i];

                const geometry = new THREE.SphereGeometry(0.1, 16, 16);
                const rgb = HSVtoRGB(color.h, color.s, color.v);
                const material = new THREE.MeshPhongMaterial({
                    color: `rgb(${rgb[0]}, ${rgb[1]}, ${rgb[2]})`,
                    emissive: `rgb(${rgb[0]/2}, ${rgb[1]/2}, ${rgb[2]/2})`
                });
                const sphere = new THREE.Mesh(geometry, material);

                sphere.position.set(position.x, position.y, position.z);
                scene.add(sphere);
            }
        }
    });

    renderer.render(scene, camera);
}

// Set up camera controls
const controls = new THREE.OrbitControls(camera, renderer.domElement);
camera.position.set(0, 0, 5);
controls.update();

// Add a grid for reference
const gridHelper = new THREE.GridHelper(10, 10);
scene.add(gridHelper);

// Animation loop
function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}
animate();