import matplotlib.pyplot as plt
import pandas as pd

# Read the data
data = pd.read_csv('NeedleForceData.csv')

# Plot the data
plt.plot(data['NeedleVerticalPosition'], data['ForceY'], label='Force Y vs Needle Position')
plt.xlabel('Needle Vertical Position')
plt.ylabel('Force Y')
plt.title('Needle Force Y vs Vertical Position')
plt.legend()
plt.grid(True)

# Show the plot
plt.show()
