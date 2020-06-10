package com.hideakin.textsearch.index.controller;

import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.service.FileGroupService;

@SpringBootTest
public class FileGroupControllerTests {

	private MockMvc mockMvc;

	@Mock
	private FileGroupService fileGroupService;

	@InjectMocks
	private FileGroupController fileGroupController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(fileGroupController).build();
    }

	@Test
	public void getGroups_successful() throws Exception {
		ValuesResponse rsp = new ValuesResponse();
		rsp.setValues(new String[] { "foo", "bar", "baz" });
		when(fileGroupService.getGroups()).thenReturn(rsp);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/groups"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$.values[0]").value(rsp.getValues()[0]))
        	.andExpect(jsonPath("$.values[1]").value(rsp.getValues()[1]))
        	.andExpect(jsonPath("$.values[2]").value(rsp.getValues()[2]));
	}

}
