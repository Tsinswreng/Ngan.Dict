// 1. 建立「值 → 值」的對照表
const ETemplateType = {
  Apple : 'Apple',
  Banana: 'Banana',
  Cherry: 'Cherry',
} as const;

// 2. 把「所有 value」收斂成一個聯合型別
type Fruit = typeof ETemplateType[keyof typeof ETemplateType]; // "Apple" | "Banana" | "Cherry"

// 3. 使用
function eat(fruit: Fruit) {
  console.log(`eating ${fruit}`);
}

eat(ETemplateType.Apple);   // ✅
eat('Banana');       // ✅

