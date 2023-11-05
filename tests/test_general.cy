var test1: int = -1;

var test2: int = -555;

var arr: array<int> = { 999, 888, 777, test2 };

for(i: int = 0; i < 4 -> increment) {
	print(arr[i])
}

for(j: int = 0; j < 4 -> increment) {
	print(j)
}

print("coi")