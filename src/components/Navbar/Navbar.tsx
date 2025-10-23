import React from 'react';
import { FaLanguage, FaCaretDown } from 'react-icons/fa';

const Navbar: React.FC = () => {
  return (
    <div className="fixed top-0 left-0 w-full h-16 flex items-center justify-between px-6 bg-white shadow z-50">

      <div className="flex items-center">
        <div className="w-6 h-6 bg-blue-500 mr-2"></div>
        <span className="text-xl font-semibold">
          Photo<span className="text-blue-500">blue</span>
        </span>
      </div>

      <div className="flex items-center space-x-6">
        <button className="bg-[#ee28ff] text-white py-1 px-4 rounded-full flex items-center">
          <span className="mr-2">✓</span> Nuevas funciones
        </button>

        <div className="flex items-center space-x-1">
          <FaLanguage />
          <span>Idioma</span>
          <FaCaretDown />
        </div>

        <span>Explorar</span>
        <span>Ayuda</span>

        <button className="border border-blue-500 text-blue-500 py-1 px-3 rounded-full">
          Cerrar sesión
        </button>

        <img
          src="https://placehold.co/400x400/jpg"
          alt="Profile"
          className="w-10 h-10 rounded-full object-cover"
        />
      </div>
    </div>
  );
};

export default Navbar;
