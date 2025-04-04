@echo off

curl -X POST http://localhost:5000/api/Core/commands/AddDriver
curl -X POST http://localhost:5000/api/Core/commands/AddVolume

curl -X POST http://localhost:5000/api/Driver/D001/commands/AddStrip
curl -X POST http://localhost:5000/api/Driver/D001/settings -H "Content-Type: application/json" -d "{\"FrameRate\": 1}"

curl -X POST http://localhost:5000/api/Volume/V001/settings -H "Content-Type: application/json" -d "{\"VolumeType\": \"Gradient\"}"

curl -X POST http://localhost:5000/api/Driver/D001/Strip/S001/settings -H "Content-Type: application/json" -d "{\"Volume\": \"V001\"}"
curl -X POST http://localhost:5000/api/Driver/D001/Strip/S001/settings -H "Content-Type: application/json" -d "{\"Layout\": \"Linear\"}"
curl -X POST http://localhost:5000/api/Driver/D001/Strip/S001/settings -H "Content-Type: application/json" -d "{\"Layout\": {\"Length\": 11}}"

curl -X POST http://localhost:5000/api/Core/settings -H "Content-Type: application/json" -d "{\"VisualizationProvider\": {\"Enabled\": true}}"
