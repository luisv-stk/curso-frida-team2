import React from 'react';
import { FaChevronDown, FaSearch } from 'react-icons/fa';

const SearchBar: React.FC = () => {
  return (
    <div className="p-12 w-screen max-h-[600px] bg-gradient-to-r from-cyan-500 to-blue-800 flex justify-center items-center">
      <div className="flex items-center bg-white shadow-md rounded overflow-hidden">
        <button className="px-4 py-2 flex items-center">
          Todos <FaChevronDown className="ml-2" />
        </button>
        <input
          type="text"
          placeholder="Buscar"
          className="py-2 px-4 flex-grow outline-none"
        />
        <button className="px-4 py-2">
          <FaSearch />
        </button>
      </div>
    </div>
  );
};

export default SearchBar;
