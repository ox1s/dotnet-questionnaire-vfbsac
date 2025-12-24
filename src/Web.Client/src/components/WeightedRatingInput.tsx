import React from 'react';
import { AlertCircle } from 'lucide-react';

interface Props {
  value: number | undefined;
  weight: number | undefined;
  onChange: (val: number | undefined, weight: number | undefined) => void;
}

export const WeightedRatingInput: React.FC<Props> = ({
  value,
  weight,
  onChange,
}) => {
  // Проверки на ошибки
  const isWeightInvalid = weight !== undefined && (weight < 1 || weight > 10);
  const isValueInvalid = value !== undefined && (value < 1 || value > 10);
  const isLogicInvalid = value !== undefined && weight !== undefined && value > weight;

  const hasError = isWeightInvalid || isValueInvalid || isLogicInvalid;

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement>,
    field: 'value' | 'weight'
  ) => {
    // Разрешаем вводить, но парсим как число. 
    // Если пусто - undefined.
    const val = e.target.value === "" ? undefined : parseFloat(e.target.value);
    
    if (field === 'weight') {
      onChange(value, val);
    } else {
      onChange(val, weight);
    }
  };

  return (
    <div className={`flex flex-col gap-2 p-4 rounded-lg border transition-colors ${hasError ? "bg-red-50 border-red-200" : "bg-gray-50 border-gray-200"}`}>
      
      <div className="flex gap-4 items-start">
        {/* Поле ВЕС */}
        <div className="flex-1">
          <label className="block text-xs font-medium text-gray-500 mb-1">
            Важность (1-10)
          </label>
          <input
            type="number"
            min="1"
            max="10"
            className={`input-field ${isWeightInvalid ? "border-red-500 ring-1 ring-red-500" : ""}`}
            placeholder="10"
            value={weight ?? ""}
            onChange={(e) => handleChange(e, 'weight')}
          />
        </div>

        <div className="text-gray-300 font-light text-2xl mt-6">/</div>

        {/* Поле ОЦЕНКА */}
        <div className="flex-1">
          <label className="block text-xs font-medium text-gray-500 mb-1">
            Оценка (1-10)
          </label>
          <input
            type="number"
            min="1"
            max="10"
            className={`input-field ${isValueInvalid || isLogicInvalid ? "border-red-500 ring-1 ring-red-500" : ""}`}
            placeholder="8"
            value={value ?? ""}
            onChange={(e) => handleChange(e, 'value')}
          />
        </div>
      </div>

      {/* Блок ошибок */}
      {hasError && (
        <div className="mt-2 flex flex-col gap-1 text-xs text-red-600 font-medium">
          {isWeightInvalid && (
            <div className="flex items-center gap-1">
              <AlertCircle size={12} /> Вес должен быть от 1 до 10
            </div>
          )}
          {isValueInvalid && (
            <div className="flex items-center gap-1">
              <AlertCircle size={12} /> Оценка должна быть от 1 до 10
            </div>
          )}
          {isLogicInvalid && !isValueInvalid && (
            <div className="flex items-center gap-1">
              <AlertCircle size={12} /> Оценка не может быть выше Важности!
            </div>
          )}
        </div>
      )}

      {!hasError && (
        <div className="text-xs text-gray-400 mt-1">
          Слева укажите важность критерия, справа — реальную оценку.
        </div>
      )}
    </div>
  );
};