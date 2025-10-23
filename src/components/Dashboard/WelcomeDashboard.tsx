import React, { useState } from 'react';
import { FaClock, FaCheck } from 'react-icons/fa';
import AddNewItem from './AddNewItem';

const Dashboard: React.FC = () => {
  const [showAddNew, setShowAddNew] = useState(false);

if (showAddNew) {
  return <AddNewItem onBack={() => setShowAddNew(false)} />;
}


  return (
    <div className="w-screen pt-24 pb-8 bg-[#f5f9fd] flex flex-col items-center justify-center">
      <div className="w-full max-w-4xl bg-[#f5f9fd] text-[#131313] ">
        <div className="flex justify-between items-center mb-4">
          <div className="flex items-center text-[#EE28FF]">
            <FaClock />
            <span className="ml-2">Última conexión: Miércol es 1 de Octubre 12:00h.</span>
          </div>
          <div className="flex items-center text-[#0179ff] cursor-pointer">
            <FaCheck />
            <span className="ml-2">Cerrar</span>
          </div>
        </div>
        <div>
          <p className="font-bold">Hola Karinne,</p>
          <p className="mb-4">Bienvenida de vuelta, en estos momentos tienes publicadas:</p>
          <ul className="list-disc pl-5 text-left mb-4">
            <li>36 fotos</li>
            <li>24 vídeos</li>
            <li>12 ilustraciones</li>
          </ul>
          <div className="flex space-x-4">
            <button className="px-4 py-2 border-2 border-[#0179ff] text-[#0179ff] rounded-lg">
              Ver todas tus publicaciones
            </button>
            <button
              onClick={() => setShowAddNew(true)}
              className="px-4 py-2 bg-[#0179ff] text-white rounded-lg"
            >
              Añadir nuevo elemento
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
