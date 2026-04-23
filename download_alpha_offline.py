import base64
import requests
import sys

cards = ["Black Lotus","Mox Sapphire","Mox Ruby","Ancestral Recall","Time Walk","Timetwister","Sol Ring","Mana Vault","Counterspell","Mahamoti Djinn","Clone","Control Magic","Lightning Bolt","Shivan Dragon","Fireball","Braingeyser","Island","Mountain"]

print("=== MTG Alpha 40 Offline Builder ===")
print("Downloading public card images from Scryfall...")

# Load your HTML file
try:
    with open("mtg-alpha-40-v3-images.html", "r", encoding="utf-8") as f:
        html = f.read()
except:
    print("ERROR: Put mtg-alpha-40-v3-images.html in same folder")
    sys.exit(1)

for card in cards:
    print(f"Fetching {card}...", end=" ")
    try:
        # Try Alpha (lea), fallback to Revised (3ed)
        url = f"https://api.scryfall.com/cards/named?exact={card.replace(' ', '%20')}&set=lea&format=image"
        r = requests.get(url, timeout=15)
        if r.status_code != 200:
            url = f"https://api.scryfall.com/cards/named?exact={card.replace(' ', '%20')}&set=3ed&format=image"
            r = requests.get(url, timeout=15)
        
        if r.status_code == 200:
            b64 = base64.b64encode(r.content).decode('utf-8')
            data_url = f"data:image/jpeg;base64,{b64}"
            # replace both http and https versions
            html = html.replace(f"https://api.scryfall.com/cards/named?exact={card.replace(' ', '%20')}&set=lea&format=image", data_url)
            html = html.replace(f"https://api.scryfall.com/cards/named?exact={card.replace(' ', '%20')}&set=3ed&format=image", data_url)
            print("✓")
        else:
            print("✗")
    except Exception as e:
        print(f"error: {e}")

with open("mtg-alpha-40-OFFLINE.html", "w", encoding="utf-8") as f:
    f.write(html)

print("\nDone! Created mtg-alpha-40-OFFLINE.html (fully offline)")
print("File size: ~8-12MB with all images embedded")
