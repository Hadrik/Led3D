const socket = new WebSocket('ws://localhost:1738');

// Storage for received data - organized by driver ID and strip ID
const ledData = {
    positions: {},
    colors: {},
    volumes: {}
};

/* Message formats:
 *  {
 *      type: "positions"
 *      data: {
 *          Id: "Driver ID"
 *          Data: [
 *              {
 *                  Id: "Strip ID"
 *                  Data: [
 *                      {
 *                          x: 1,
 *                          y: 1,
 *                          z: 1
 *                      }
 *                  ]
 *              }
 *          ]
 *      }
 *  }
 * 
 * {
 *      type: "colors",
 *      data: {
 *          Id: "Driver ID"
 *          Data: [
 *              {
 *                  Id: "Strip ID"
 *                  Data: [
 *                      {
 *                          H: 100,
 *                          S: 1,
 *                          V: 1
 *                      }
 *                  ]
 *              }
 *          ]
 *      }
 *  }
 * 
 * {
 *      type: "volume",
 *      data: {
 *          Id: "Volume ID"
 *          Position: Vector3
 *          Rotation: Vector3
 *          Scale: Vector3
 *      }
 *  }
 */
socket.onmessage = (event) => {
    const message = JSON.parse(event.data);

    if (message.type === "positions") {
        // Process position data
        const driver = message.data;
        const driverId = driver.Id;

        if (!ledData.positions[driverId]) {
            ledData.positions[driverId] = {};
        }

        driver.Data.forEach(strip => {
            const stripId = strip.Id;
            ledData.positions[driverId][stripId] = strip.Data;
        });
        renderScene();
    } else if (message.type === "colors") {
        // Process color data
        let driver = message.data;
        const driverId = driver.Id;

        if (!ledData.colors[driverId]) {
            ledData.colors[driverId] = {};
        }

        driver.Data.forEach(strip => {
            const stripId = strip.Id;
            strip.Data.forEach(pixel => {
                const conv = hsv2rgb(pixel.H, pixel.S / 255, pixel.V / 255)
                pixel.R = Math.round(conv[0] * 100);
                pixel.G = Math.round(conv[1] * 100);
                pixel.B = Math.round(conv[2] * 100);
            });
            ledData.colors[driverId][stripId] = strip.Data;
        });
        renderScene();
    } else if (message.type === "volume") {
        ledData.volumes[message.data.Id] = {
            Position: message.data.Position,
            Rotation: message.data.Rotation,
            Scale: message.data.Scale
        }
        renderScene();
    }
};

const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
const renderer = new THREE.WebGLRenderer();

renderer.setSize(window.innerWidth, window.innerHeight);
document.body.appendChild(renderer.domElement);

// input: h in [0,360] and s,v in [0,1] - output: r,g,b in [0,1]
function hsv2rgb(h,s,v)
{
    let f= (n,k=(n+h/60)%6) => v - v*s*Math.max( Math.min(k,4-k,1), 0);
    return [f(5),f(3),f(1)];
}
function renderScene() {
    // Clear existing objects
    while (scene.children.length > 0) {
        scene.remove(scene.children[0]);
    }

    // Add a light if none exists
    if (!scene.children.some(child => child.isLight)) {
        const ambientLight = new THREE.AmbientLight(0x404040);
        const pointLight = new THREE.PointLight(0xffffff, 1, 100);
        pointLight.position.set(5, 5, 5);
        scene.add(ambientLight);
        scene.add(pointLight);
    }

    // For each driver
    Object.keys(ledData.positions).forEach(driverId => {
        if (!ledData.colors[driverId]) return;

        // For each strip in the driver
        Object.keys(ledData.positions[driverId]).forEach(stripId => {
            const positions = ledData.positions[driverId][stripId];
            const colors = ledData.colors[driverId]?.[stripId];

            // Only render if we have both position and color data for this strip
            if (positions && colors && positions.length === colors.length) {
                for (let i = 0; i < positions.length; i++) {
                    const position = positions[i];
                    const color = colors[i];

                    const geometry = new THREE.SphereGeometry(0.1, 16, 16);
                    const material = new THREE.MeshPhongMaterial({
                        color: `rgb(${color.R}%, ${color.G}%, ${color.B}%)`,
                        // emissive: `rgb(${color.R / 2}%, ${color.G / 2}%, ${color.B / 2}%)`
                    });
                    const sphere = new THREE.Mesh(geometry, material);

                    sphere.position.set(position.x, position.y, position.z);
                    scene.add(sphere);
                }
            }
        });
    });
    
    // For each volume
    Object.keys(ledData.volumes).forEach(volumeId => {
        const volume = ledData.volumes[volumeId];
        const geometry = new THREE.BoxGeometry(volume.Scale.x, volume.Scale.y, volume.Scale.z);
        const wireframe = new THREE.WireframeGeometry(geometry);
        const edges = new THREE.LineSegments(wireframe);
        edges.material.depthTest = false;
        edges.material.transparent = true;
        edges.material.opacity = 0.25;
        edges.material.color.set(0x00ff00);
        edges.position.set(volume.Position.x, volume.Position.y, volume.Position.z);
        edges.rotation.set(volume.Rotation.x, volume.Rotation.y, volume.Rotation.z);
        scene.add(edges);
        // const box = new THREE.Mesh(geometry, new THREE.MeshBasicMaterial({
        //     color: 0x00ff00,
        //     wireframe: true
        // }));
        // box.position.set(volume.Position.x, volume.Position.y, volume.Position.z);
        // box.rotation.set(volume.Rotation.x, volume.Rotation.y, volume.Rotation.z);
        // scene.add(box);
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