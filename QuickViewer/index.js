const socket = new WebSocket('ws://localhost:5432');

socket.onmessage = (event) => {
    const data = JSON.parse(event.data); // Assume data is an array of objects with position and HSV color
    updateScene(data);
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

function updateScene(data) {
    // Clear existing objects
    while (scene.children.length > 0) {
        scene.remove(scene.children[0]);
    }

    data.forEach(({ position, color }) => {
        // Create a sphere for each data point
        const geometry = new THREE.SphereGeometry(0.5, 32, 32);
        const rgb = HSVtoRGB(color.h, color.s, color.v);
        const material = new THREE.MeshBasicMaterial({ color: `rgb(${rgb[0]}, ${rgb[1]}, ${rgb[2]})` });
        const sphere = new THREE.Mesh(geometry, material);

        sphere.position.set(position.x, position.y, position.z);
        scene.add(sphere);
    });

    renderer.render(scene, camera);
}

camera.position.z = 5;

// Animation loop
function animate() {
    requestAnimationFrame(animate);
    renderer.render(scene, camera);
}
animate();