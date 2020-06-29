package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.service.FileService;

@RestController
public class FileController {

	@Autowired
	private FileService service;

	@RequestMapping(value="/v1/files",method=RequestMethod.GET)
	public ResponseEntity<?> getFiles() {
		String[] paths = service.getFiles("default");
		if (paths != null) {
			return new ResponseEntity<>(new ValuesResponse(paths), HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{group}",method=RequestMethod.GET)
	public ResponseEntity<?> getFilesByGroup(
			@PathVariable String group) {
		String[] paths = service.getFiles(group);
		if (paths != null) {
			return new ResponseEntity<>(new ValuesResponse(paths), HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
