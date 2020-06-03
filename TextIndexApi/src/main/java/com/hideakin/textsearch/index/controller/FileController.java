package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
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

	@RequestMapping(value="/files",method=RequestMethod.GET)
	public ValuesResponse getFiles() {
		return service.getFiles("default");
	}

	@RequestMapping(value="/files/{group}",method=RequestMethod.GET)
	public ValuesResponse getFilesByGroup(
			@PathVariable String group) {
		return service.getFiles(group);
	}

}
